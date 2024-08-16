/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年8月14 10:23
 * function    :
 * ===============================================
 * */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AppFrame.Editor
{
    [CustomEditor(typeof(TreeView), true)]
    public class TreeViewEditor : UnityEditor.Editor
    {
        private const string kStandardSpritePath       = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpritePath     = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath                 = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath            = "UI/Skin/Checkmark.psd";
        private const string kDropdownArrowPath        = "UI/Skin/DropdownArrow.psd";
        private const string kMaskPath                 = "UI/Skin/UIMask.psd";

        static private DefaultControls.Resources s_StandardResources;

        static private DefaultControls.Resources GetStandardResources()
        {
            if (s_StandardResources.standard == null)
            {
                s_StandardResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>(kStandardSpritePath);
                s_StandardResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>(kBackgroundSpritePath);
                s_StandardResources.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>(kInputFieldBackgroundPath);
                s_StandardResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>(kKnobPath);
                s_StandardResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>(kCheckmarkPath);
                s_StandardResources.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>(kDropdownArrowPath);
                s_StandardResources.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>(kMaskPath);
            }
            return s_StandardResources;
        }
        private SerializedObject _target;
        
        private SerializedProperty isToggle;
        private SerializedProperty itemParent;
        private SerializedProperty item;
        private SerializedProperty itemHeight;
        private SerializedProperty enterColor;
        private SerializedProperty clickColor;
        
        
        private TreeView treeView;
        
        void OnEnable()
        {
            _target = new SerializedObject(target);
            
            treeView = (TreeView)target;
            
            isToggle = _target.FindProperty("isToggle");
            itemParent = _target.FindProperty("itemParent");
            item = _target.FindProperty("item");
            itemHeight = _target.FindProperty("itemHeight");
            enterColor = _target.FindProperty("enterColor");
            clickColor = _target.FindProperty("clickColor");
        }
        
        [MenuItem("GameObject/UI/TreeView")]
        public static void TreeView()
        {
            GameObject parent = GetOrCreateCanvasGameObject();

            GameObject go = new GameObject("TreeView", typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<Image>();
            go.layer = 5;

            #region ScrollView

            var scroll = DefaultControls.CreateScrollView(GetStandardResources());
            scroll.transform.SetParent(go.transform, false);
            var scrollTransform = scroll.GetComponent<RectTransform>();
            scrollTransform.anchorMin = Vector2.zero;
            scrollTransform.anchorMax = Vector2.one;
            scrollTransform.offsetMin = Vector2.zero;
            scrollTransform.offsetMax = Vector2.zero;

            var scrollview = scroll.GetComponent<ScrollRect>();
            var vlg = scrollview.content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childScaleWidth = true;
            vlg.childScaleHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;

            var size = scrollview.content.gameObject.AddComponent<ContentSizeFitter>();
            size.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            size.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            #endregion

            #region Template

            var template = new GameObject("Template", typeof(RectTransform), typeof(CanvasRenderer));
            template.transform.SetParent(go.transform, false);
            template.layer = 5;
            var tvlg = template.AddComponent<VerticalLayoutGroup>();
            tvlg.childAlignment = TextAnchor.MiddleLeft;
            tvlg.childControlWidth = true;
            tvlg.childControlHeight = true;
            tvlg.childScaleWidth = true;
            tvlg.childScaleHeight = false;
            tvlg.childForceExpandWidth = false;
            tvlg.childForceExpandHeight = false;
            template.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 20);
            
            var root = new GameObject("Root", typeof(RectTransform), typeof(CanvasRenderer));
            root.AddComponent<Image>().color = Color.clear;
            root.transform.SetParent(template.transform, false);
            root.layer = 5;
            var rvlg = root.AddComponent<HorizontalLayoutGroup>();
            rvlg.spacing = 2;
            rvlg.childAlignment = TextAnchor.MiddleLeft;
            rvlg.childControlWidth = false;
            rvlg.childControlHeight = false;
            rvlg.childScaleWidth = true;
            rvlg.childScaleHeight = false;
            rvlg.childForceExpandWidth = false;
            rvlg.childForceExpandHeight = false;
            
            var btn = DefaultControls.CreateButton(GetStandardResources());
            DestroyImmediate(btn.transform.GetChild(0).gameObject);
            btn.transform.SetParent(root.transform, false);
            btn.GetComponent<RectTransform>().sizeDelta = Vector2.one * 20;
            btn.name = "Button";

            var toggle = DefaultControls.CreateToggle(GetStandardResources());
            DestroyImmediate(toggle.transform.Find("Label").gameObject);
            toggle.transform.SetParent(root.transform, false);
            toggle.GetComponent<RectTransform>().sizeDelta = Vector2.one * 20;
            var bg = toggle.transform.GetChild(0).GetComponent<RectTransform>();
            var cm = toggle.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
            cm.anchorMin = Vector2.zero;
            cm.anchorMax = Vector2.one;
            cm.offsetMin = Vector2.zero;
            cm.offsetMax = Vector2.zero;
            
            var image = DefaultControls.CreateImage(GetStandardResources());
            image.transform.SetParent(root.transform, false);
            image.GetComponent<RectTransform>().sizeDelta = Vector2.one * 20;
            image.GetComponent<Image>().raycastTarget = false;
            
            var text = DefaultControls.CreateText(GetStandardResources());
            text.transform.SetParent(root.transform, false);
            text.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 20);
            text.GetComponent<Text>().raycastTarget = false;
            text.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            text.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            text.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;
            text.name = "Text";
            
            #endregion

            var treeView = go.AddComponent<TreeView>();
            treeView.isToggle = true;
            treeView.itemParent = scrollview.content;
            treeView.item = template;
            treeView.itemHeight = 20;
            treeView.enterColor = Color.gray;
            treeView.clickColor = Color.blue;
            
            template.SetActive(false);
            
            Selection.activeGameObject = go;
        }

        // Update is called once per frame
        public override void OnInspectorGUI()
        { 
            base.OnInspectorGUI();
            Undo.RecordObject(treeView, "object change");

            EditorGUILayout.PropertyField(isToggle);
            EditorGUILayout.PropertyField(item);
            EditorGUILayout.PropertyField(itemParent);
            EditorGUILayout.PropertyField(itemHeight);
            EditorGUILayout.PropertyField(enterColor);
            EditorGUILayout.PropertyField(clickColor);
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
            _target.ApplyModifiedProperties();
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