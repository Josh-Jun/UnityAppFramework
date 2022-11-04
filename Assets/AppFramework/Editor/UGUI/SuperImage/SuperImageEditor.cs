using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.SceneManagement;

namespace AppFramework.Editor
{
    [CustomEditor(typeof(SuperImage), true)]
    public class SuperImageEditor : UnityEditor.Editor
    {
        private SerializedProperty sprite;
        private SerializedProperty color;
        private SerializedProperty material;
        private SerializedProperty raycastTarget;
        private SerializedProperty rayShapeFilter;
        private SerializedProperty segements;
        private SerializedProperty round;

        void OnEnable()
        {
            sprite = serializedObject.FindProperty("m_Sprite");
            color = serializedObject.FindProperty("m_Color");
            material = serializedObject.FindProperty("m_Material");
            raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            rayShapeFilter = serializedObject.FindProperty("rayShapeFilter");
            segements = serializedObject.FindProperty("segements");
            round = serializedObject.FindProperty("round");
        }

        [MenuItem("GameObject/UI/SuperImage")]
        public static void SuperImage()
        {
            GameObject parent = GetOrCreateCanvasGameObject();

            GameObject go = new GameObject("SuperImage");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            go.AddComponent<SuperImage>();
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

            EditorGUILayout.PropertyField(rayShapeFilter);
            EditorGUILayout.PropertyField(round);
            EditorGUILayout.PropertyField(segements);

            serializedObject.ApplyModifiedProperties();
        }

        static bool IsValidCanvas(Canvas canvas)
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

        static GameObject GetOrCreateCanvasGameObject()
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

        static GameObject CreateNewUI()
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

        static void CreateEventSystem(bool select, GameObject parent)
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
