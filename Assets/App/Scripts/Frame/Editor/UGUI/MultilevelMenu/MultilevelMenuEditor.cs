/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年8月21 14:48
 * function    : 
 * ===============================================
 * */

using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    public class MultilevelMenuEditor : UnityEditor.Editor
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

        [MenuItem("GameObject/UI/MultilevelMenu")]
        public static void MultilevelMenu()
        {
            var parent = GetOrCreateCanvasGameObject();

            var go = DefaultControls.CreateButton(GetStandardResources());
            go.name = "Multilevel Menu";
            go.transform.SetParent(parent.transform, false);
            var go_rt = go.GetComponent<RectTransform>();
            go_rt.sizeDelta = new Vector2(120, 30);
            DestroyImmediate(go.transform.GetChild(0).gameObject);

            var root = DefaultControls.CreateImage(GetStandardResources());
            root.name = "Root";
            var root_rt = root.GetComponent<RectTransform>();
            root_rt.SetParent(go_rt, false);
            root_rt.anchorMin = Vector2.zero;
            root_rt.anchorMax = Vector2.zero;
            root_rt.pivot = Vector2.up;
            var vlg = root.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.spacing = 4;
            vlg.padding = new RectOffset(4, 4, 4, 4);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childScaleWidth = true;
            vlg.childScaleHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var csf = root.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            var item = DefaultControls.CreateImage(GetStandardResources());
            item.name = "Item";
            item.transform.SetParent(root_rt, false);
            var hlg = item.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childScaleWidth = true;
            hlg.childScaleHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var text = DefaultControls.CreateText(GetStandardResources());
            text.name = "Text";
            text.transform.SetParent(item.transform, false);
            text.GetComponent<Text>().raycastTarget = false;
            text.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            text.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Overflow;
            text.GetComponent<Text>().verticalOverflow = VerticalWrapMode.Overflow;

            var le = text.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            var image = DefaultControls.CreateImage(GetStandardResources());
            image.name = "Image";
            image.GetComponent<Image>().raycastTarget = false;
            image.transform.SetParent(item.transform, false);

            var menu = go.AddComponent<MultilevelMenu>();
            menu.root = root;
            menu.item = item;
            
            root.SetActive(false);
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
