/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年3月17 16:44
 * function    : 
 * ===============================================
 * */
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(ImagePro), true)]
    public class ImageProEditor : Editor
    {
        private ImagePro targetComponent;
        private SerializedProperty sprite;
        private SerializedProperty color;
        private SerializedProperty material;
        private SerializedProperty raycastTarget;
        
        private SerializedProperty radius;

        private void OnEnable()
        {
            EditorApplication.update += Excute;
            targetComponent = target as ImagePro;
            sprite = serializedObject.FindProperty("m_Sprite");
            color = serializedObject.FindProperty("m_Color");
            material = serializedObject.FindProperty("m_Material");
            raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            radius = serializedObject.FindProperty("_Radius");
        }

        [MenuItem("GameObject/UI/ImagePro")]
        public static void ImagePro()
        {
            GameObject parent = GetOrCreateCanvasGameObject();

            GameObject go = new GameObject("ImagePro");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            go.AddComponent<ImagePro>();
            go.layer = 5;
            Selection.activeGameObject = go;
        }

        // Update is called once per frame
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(sprite);
            EditorGUILayout.PropertyField(color);
            EditorGUILayout.PropertyField(material);
            EditorGUILayout.PropertyField(raycastTarget);

            EditorGUILayout.PropertyField(radius);

            serializedObject.ApplyModifiedProperties();
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
            GameObject selected = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = selected?.GetComponentInParent<Canvas>();
            if (IsValidCanvas(canvas))
                return selected;

            // No canvas in selection or its parents? Then use any valid canvas.
            // We have to find all loaded Canvases, not just the ones in main scenes.
            Canvas[] canvasArray = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Canvas>();
            for (int i = 0; i < canvasArray.Length; i++)
                if (IsValidCanvas(canvasArray[i]))
                    return canvasArray[i].gameObject;

            // No canvas in the scene at all? Then create a new one.
            return CreateNewUI();
        }

        private static GameObject CreateNewUI()
        {
            // Root for the UI
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            bool customScene = false;
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
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
            StageHandle stage =
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
