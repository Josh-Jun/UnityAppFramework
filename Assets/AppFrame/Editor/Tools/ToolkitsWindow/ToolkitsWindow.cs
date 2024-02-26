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

        [MenuItem("Tools/ToolkitsWindow", false, 1)]
        public static void ShowExample()
        {
            var icon = EditorGUIUtility.IconContent("Assembly Icon").image;
            ToolkitsWindow wnd = GetWindow<ToolkitsWindow>();
            wnd.titleContent = new GUIContent("Toolkits", icon);
        }

        public void CreateGUI()
        {
            itemsName = GetItemsName();
            stamp = PlayerPrefs.HasKey(STAMP_KEY) ? PlayerPrefs.GetInt(STAMP_KEY) : 0;
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;

            // Import UXML
            var visualTree = EditorTool.GetEditorAsset<VisualTreeAsset>($"ToolkitsWindow.uxml");
            visualTree.CloneTree(root);

            infos = root.Q<ScrollView>("infos");
            view_title = root.Q<Label>("view_title");
            view_title.text = itemsName[0];

            var right = root.Q<VisualElement>("right");
            for (int i = 0; i < itemsName.Length; i++)
            {
                var viewItem = EditorTool.GetEditorAsset<VisualTreeAsset>($"{itemsName[i]}/{itemsName[i]}.uxml");
                var view = viewItem.CloneTree();
                view.style.display = DisplayStyle.None;
                viewElements.Add(view);
                right.Add(view);
            }

            leftListView = root.Q<ListView>("left");
            leftListView.itemsSource = itemsName;
            leftListView.makeItem = MakeListItem;
            leftListView.bindItem = BindListItem;
            leftListView.onSelectedIndicesChange += OnItemsChosen;
            leftListView.SetSelection(stamp);

            for (int i = 0; i < itemsName.Length; i++)
            {
                IToolkitEditor editor = EditorTool.GetEditor(itemsName[i]);
                editor.OnCreate(root);
            }
        }

        private string[] GetItemsName()
        {
            List<string> list = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(EditorTool.BaseDataPath);
            DirectoryInfo[] directory = directoryInfo.GetDirectories("*");
            foreach (var info in directory)
            {
                var files = info.GetFiles("*.cs");
                if (files.Length > 0)
                {
                    var filename = Path.GetFileNameWithoutExtension(files[0].FullName);
                    list.Add(filename);
                }
            }
            return list.ToArray();
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
            label.name = index.ToString();
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