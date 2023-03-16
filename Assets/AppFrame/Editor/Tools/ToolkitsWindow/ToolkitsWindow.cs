using System.Collections.Generic;
using AppFrame.Enum;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;

namespace AppFrame.Editor
{
    public class ToolkitsWindow : EditorWindow
    {
        private const string basePath = "Assets/AppFrame/Editor/Tools/ToolkitsWindow";
        private VisualElement root;
        private ListView leftListView;
        private Label view_title;

        private string[] itemsName =
        {
            "BuildApp",
            "BuildAssetBundle",
            "SetAppScriptConfig",
            "SetAppTableConfig",
            "Table2CSharp",
            "ChangePrefabsFont",
            "FindSameFileName",
        };

        private List<TemplateContainer> viewElements = new List<TemplateContainer>();
        private int stamp = 0;

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
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{basePath}/ToolkitsWindow.uxml");
            visualTree.CloneTree(root);

            leftListView = root.Q<ListView>("left");
            // leftListView.
            leftListView.itemsSource = itemsName;
            leftListView.makeItem = MakeListItem;
            leftListView.bindItem = BindListItem;
            leftListView.onSelectedIndicesChange += OnItemsChosen;

            view_title = root.Q<Label>("view_title");
            view_title.text = itemsName[0];

            var right = root.Q<VisualElement>("right");
            for (int i = 0; i < itemsName.Length; i++)
            {
                var viewItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{basePath}/{itemsName[i]}/{itemsName[i]}.uxml");
                var view = viewItem.CloneTree();
                view.style.display = DisplayStyle.None;
                viewElements.Add(view);
                right.Add(view);
            }

            viewElements[stamp].style.display = DisplayStyle.Flex;

            BuildAppFunction();
            BuildAssetBundleFunction();
            SetAppScriptConfigFunction();
            SetAppTableConfigFunction();
            Table2CSharpFunction();
            ChangePrefabsFontFunction();
            FindSameFileNameFunction();

            infos = root.Q<ScrollView>("infos");
        }

        private void BuildAppFunction()
        {
            
        }
        private void BuildAssetBundleFunction()
        {
            
        }
        private void SetAppScriptConfigFunction()
        {
            var popup_parent = root.Q<VisualElement>("popup_parent");
            var script_scroll_view = root.Q<ScrollView>("script_scroll_view");
            var script_list = SetAppScriptConfig.GetRootScripts();
            var mainScenePopup = new PopupField<string>("MainSceneName", SetAppScriptConfig.SceneNames, SetAppScriptConfig.level);
            mainScenePopup.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                SetAppScriptConfig.level = SetAppScriptConfig.SceneNames.IndexOf(evt.newValue);
            });
            popup_parent.Add(mainScenePopup);
            
            RefreshScriptView(script_scroll_view);
            root.Q<Button>("btn_script_apply").clicked += SetAppScriptConfig.ApplyConfig;
        }
        private void SetAppTableConfigFunction()
        {
            var table_scroll_view = root.Q<ScrollView>("table_scroll_view");
            RefreshTableView(table_scroll_view);
            root.Q<Button>("btn_table_apply").clicked += SetAppTableConfig.ApplyConfig;
        }
        private void Table2CSharpFunction()
        {
            
        }
        private void ChangePrefabsFontFunction()
        {
            var textPrefabsField = root.Q<TextField>("text_prefab_path");
            textPrefabsField.value = "";
            var fontObjectField = root.Q<ObjectField>("object_font");
            fontObjectField.objectType = typeof(Font);

            root.Q<Button>("prefab_path_browse").clicked += () =>
            {
                textPrefabsField.value = Browse();
            };

            root.Q<Button>("change_font_apply").clicked += () =>
            {
                ChangePrefabsFont.ChangeFont((Font)fontObjectField.value, textPrefabsField.value);
            };
        }
        private void FindSameFileNameFunction()
        {
            var objectType = root.Q<EnumField>("object_type");
            objectType.Init(ObjectType.All);
            var textFilesField = root.Q<TextField>("text_files_path");
            textFilesField.value = "";
            
            root.Q<Button>("files_path_browse").clicked += () =>
            {
                textFilesField.value = Browse();
            };

            root.Q<Button>("btn_find").clicked += () =>
            {
                FindSameFileName.FindSameFile((ObjectType)objectType.value, textFilesField.value);
            };
        }

        private void RefreshTableView(ScrollView table_scroll_view)
        {
            var table_list = SetAppTableConfig.GetAppTables();
            
            if (table_list.Count > 0)
            {
                table_scroll_view.Clear();
                for (int i = 0; i < table_list.Count; i++)
                {
                    int index = i;
                    var tableItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{basePath}/SetAppTableConfig/table_item.uxml");
                    VisualElement table = tableItem.CloneTree();
                    table.Q<EnumField>("TableMold").Init(table_list[index].TableMold);
                    table.Q<TextField>("TableName").value = table_list[index].TableName;
                    table.Q<Button>("btn_table_remove").clicked += () =>
                    {
                        SetAppTableConfig.RemoveTable(index);
                        RefreshTableView(table_scroll_view);
                    };
                    table.Q<Button>("btn_table_add").clicked += () =>
                    {
                        SetAppTableConfig.AddTable();
                        RefreshTableView(table_scroll_view);
                    };
                    table_scroll_view.Add(table);
                }
            }
        }
        private void RefreshScriptView(ScrollView table_script_view)
        {
            var script_list = SetAppScriptConfig.GetRootScripts();
            
            if (script_list.Count > 0)
            {
                table_script_view.Clear();
                for (int i = 0; i < script_list.Count; i++)
                {
                    int index = i;
                    var scriptItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{basePath}/SetAppScriptConfig/script_item.uxml");
                    VisualElement script = scriptItem.CloneTree();
                    
                    var popup_parent = script.Q<VisualElement>("Popup");
                    var popup = new PopupField<string>("", SetAppScriptConfig.SceneNames, SetAppScriptConfig.SceneNames.IndexOf(script_list[index].SceneName));
                    popup_parent.Add(popup);
                    
                    script.Q<TextField>("ScriptName").value = script_list[index].ScriptName;
                    script.Q<Button>("btn_script_remove").clicked += () =>
                    {
                        SetAppScriptConfig.Remove(index);
                        RefreshScriptView(table_script_view);
                    };
                    script.Q<Button>("btn_script_add").clicked += () =>
                    {
                        SetAppScriptConfig.Add();
                        RefreshScriptView(table_script_view);
                    };
                    table_script_view.Add(script);
                }
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
            var helpBox = new HelpBox(msg, HelpBoxMessageType.Info);
            infos.Add(helpBox);
        }
        
        public static string Browse()
        {
            var newPath = EditorUtility.OpenFolderPanel("Prefabs Folder", Application.dataPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
            }

            return newPath;
        }
    }
}