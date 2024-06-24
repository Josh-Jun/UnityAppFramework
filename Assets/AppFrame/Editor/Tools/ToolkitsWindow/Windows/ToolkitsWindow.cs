using System;
using System.Collections.Generic;
using System.IO;
using AppFrame.Config;
using AppFrame.Enum;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Toggle = UnityEngine.UIElements.Toggle;

namespace AppFrame.Editor
{
    public class ToolkitsWindow : EditorWindow
    {
        private VisualElement root;
        private ListView leftListView;
        private Label view_title;

        private string[] itemsName;

        private List<TemplateContainer> viewElements = new List<TemplateContainer>();
        private int stamp = 0;
        private const string STAMP_KEY = "EDITOR_TOOLKITS_STAMP";

        private static ScrollView infos;

        private List<IToolkitEditor> editors = new List<IToolkitEditor>();

        [MenuItem("Tools/ToolkitsWindow", false, 1)]
        public static void ShowWindow()
        {
            var icon = EditorGUIUtility.IconContent("Assembly Icon").image;
            ToolkitsWindow window = GetWindow<ToolkitsWindow>();
            window.titleContent = new GUIContent("Toolkits", icon);
            window.SaveChanges();
        }

        public void CreateGUI()
        {
            itemsName = EditorTool.GetScriptName("App.Frame.Editor","IToolkitEditor", false).ToArray();
            stamp = PlayerPrefs.HasKey(STAMP_KEY) ? PlayerPrefs.GetInt(STAMP_KEY) : 0;
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;

            // Import UXML
            var visualTree = EditorTool.GetEditorWindowsAsset<VisualTreeAsset>($"ToolkitsWindow.uxml");
            visualTree.CloneTree(root);

            infos = root.Q<ScrollView>("infos");
            view_title = root.Q<Label>("view_title");
            view_title.text = itemsName[0];

            var right = root.Q<VisualElement>("right");
            for (int i = 0; i < itemsName.Length; i++)
            {
                var viewItem = EditorTool.GetEditorWindowsAsset<VisualTreeAsset>($"{itemsName[i]}/{itemsName[i]}.uxml");
                var view = viewItem.CloneTree();
                view.style.display = DisplayStyle.None;
                viewElements.Add(view);
                right.Add(view);
            }

            leftListView = root.Q<ListView>("left");
            leftListView.itemsSource = itemsName;
            leftListView.makeItem = MakeListItem;
            leftListView.bindItem = BindListItem;
            leftListView.selectionType = SelectionType.Single;
            leftListView.selectedIndicesChanged += OnItemsChosen;
            leftListView.SetSelection(stamp);

            for (int i = 0; i < itemsName.Length; i++)
            {
                IToolkitEditor editor = EditorTool.GetEditor(itemsName[i]);
                editor.OnCreate(root);
                editors.Add(editor);
            }
        }

        public void Update()
        {
            for (int i = 0; i < editors.Count; i++)
            {
                editors[i].OnUpdate();
            }
        }

        public void OnDestroy()
        {
            for (int i = 0; i < editors.Count; i++)
            {
                editors[i].OnDestroy();
            }
        }

        private void OnItemsChosen(IEnumerable<int> objs)
        {
            foreach (var index in objs)
            {
                infos.Clear();
                view_title.text = itemsName[index];
                viewElements[stamp].style.display = DisplayStyle.None;
                viewElements[index].style.display = DisplayStyle.Flex;
                stamp = index;
                PlayerPrefs.SetInt(STAMP_KEY, stamp);
            }
        }

        private void BindListItem(VisualElement ve, int index)
        {
            var label = ve as Label;
            label.text = itemsName[index];
        }

        private VisualElement MakeListItem()
        {
            var label = new Label();
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.paddingLeft = 16;
            return label;
        }

        public static void ShowHelpBox(string msg, HelpBoxMessageType type = HelpBoxMessageType.Info)
        {
            var helpBox = new HelpBox(msg, type);
            infos.Add(helpBox);
        }
    }
}