/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年3月17 16:44
 * function    : 
 * ===============================================
 * */
using UnityEngine;
using UnityEditor.AnimatedValues;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ImagePro), true)]
    [CanEditMultipleObjects]
    public class ImageProEditor : ImageEditor
    {
        private SerializedProperty radius;
        private ImagePro targetComponent;
        
        SerializedProperty m_Sprite;
        SerializedProperty m_PreserveAspect;
        SerializedProperty m_UseSpriteMesh;
        SerializedProperty m_Type;
        AnimBool m_ShowTypePro;
        bool m_bIsDrivenPro;
        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.update += Excute;
            targetComponent = target as ImagePro;
            radius = serializedObject.FindProperty("_Radius");
            
            m_Sprite                = serializedObject.FindProperty("m_Sprite");
            m_Type                  = serializedObject.FindProperty("m_Type");
            m_PreserveAspect        = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh         = serializedObject.FindProperty("m_UseSpriteMesh");
            
            m_ShowTypePro = new AnimBool(m_Sprite.objectReferenceValue != null);
            m_ShowTypePro.valueChanged.AddListener(Repaint);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorApplication.update -= Excute;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var rect = targetComponent.GetComponent<RectTransform>();
            m_bIsDrivenPro = (rect.drivenByObject as Slider)?.fillRect == rect;

            SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(radius);
            MaskableControlsGUI();

            m_ShowTypePro.target = m_Sprite.objectReferenceValue != null;
            if (EditorGUILayout.BeginFadeGroup(m_ShowTypePro.faded))
                TypeGUI();
            EditorGUILayout.EndFadeGroup();

            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;

                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Simple)
                    EditorGUILayout.PropertyField(m_UseSpriteMesh);

                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }

        private void SetShowNativeSize(bool instant)
        {
            var type = (Image.Type)m_Type.enumValueIndex;
            var showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) && m_Sprite.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }

        private void Excute()
        {
            if (targetComponent != null)
            {
                var method = targetComponent.GetType().GetMethod("Refresh",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(targetComponent, null);
                }
            }
        }

        [MenuItem("GameObject/UI/ImagePro")]
        public static void ImagePro()
        {
            var parent = GetOrCreateCanvasGameObject();

            var go = new GameObject("ImagePro");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            go.AddComponent<ImagePro>();
            go.layer = 5;
            Selection.activeGameObject = go;
        }
        
        private static bool IsValidCanvas(Canvas canvas)
        {
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                return false;

            // It's important that the non-editable canvas from a prefab scene won't be rejected,
            // but canvases not visible in the Hierarchy at all do. Don't check for HideAndDontSave.
            if (EditorUtility.IsPersistent(canvas) || (canvas.hideFlags & HideFlags.HideInHierarchy) != 0)
                return false;

            if (StageUtility.GetStageHandle(canvas.gameObject) != StageUtility.GetCurrentStageHandle())
                return false;

            return true;
        }

        private static GameObject GetOrCreateCanvasGameObject()
        {
            var selected = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            var canvas = selected?.GetComponentInParent<Canvas>();
            if (IsValidCanvas(canvas))
                return selected;

            // No canvas in selection or its parents? Then use any valid canvas.
            // We have to find all loaded Canvases, not just the ones in main scenes.
            var canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            foreach (var t in canvasArray)
                if (IsValidCanvas(t))
                    return t.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        private static GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas")
            {
                layer = LayerMask.NameToLayer("UI")
            };
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            var customScene = false;
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                root.transform.SetParent(prefabStage.prefabContentsRoot.transform, false);
                customScene = true;
            }

            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            // If there is no event system add one...
            // No need to place event system in custom scene as these are temporary anyway.
            // It can be argued for or against placing it in the user scenes,
            // but let's not modify scene user is not currently looking at.
            if (!customScene)
                CreateEventSystem(false, null);
            return root;
        }

        private static void CreateEventSystem(bool select, GameObject parent)
        {
            var stage =
                parent == null ? StageUtility.GetCurrentStageHandle() : StageUtility.GetStageHandle(parent);
            var esys = stage.FindComponentOfType<EventSystem>();
            if (esys == null)
            {
                var eventSystem = new GameObject("EventSystem");
                if (parent == null)
                    StageUtility.PlaceGameObjectInCurrentStage(eventSystem);
                else
                    GameObjectUtility.SetParentAndAlign(eventSystem, parent);
                esys = eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
            }

            if (select && esys != null)
            {
                Selection.activeGameObject = esys.gameObject;
            }
        }
    }
}
