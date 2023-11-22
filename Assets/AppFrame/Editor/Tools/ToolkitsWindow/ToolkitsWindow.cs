using System;
using System.Collections.Generic;
using System.IO;
using AppFrame.Config;
using AppFrame.Enum;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

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
            "CopyTemplateScripts",
        };

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
            stamp = PlayerPrefs.HasKey(STAMP_KEY) ? PlayerPrefs.GetInt(STAMP_KEY) : 0;
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{basePath}/ToolkitsWindow.uxml");
            visualTree.CloneTree(root);

            infos = root.Q<ScrollView>("infos");
            view_title = root.Q<Label>("view_title");
            view_title.text = itemsName[0];

            var right = root.Q<VisualElement>("right");
            for (int i = 0; i < itemsName.Length; i++)
            {
                var viewItem =
                    AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{basePath}/{itemsName[i]}/{itemsName[i]}.uxml");
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

            BuildAppFunction();
            BuildAssetBundleFunction();
            SetAppScriptConfigFunction();
            SetAppTableConfigFunction();
            Table2CSharpFunction();
            ChangePrefabsFontFunction();
            FindSameFileNameFunction();
            CopyTemplateScriptsFunction();
        }

        private void BuildAppFunction()
        {
            BuildApp.Init();
            var development_build = root.Q<Toggle>("DevelopmentBuild");
            var is_test_server = root.Q<Toggle>("IsTestServer");
            var load_assets_mold = root.Q<EnumField>("LoadAssetsMold");
            var app_frame_rate = root.Q<TextField>("AppFrameRate");
            var build_mold = root.Q<EnumField>("BuildMold");
            var export_project = root.Q<Toggle>("ExportProject");
            var ab_build_pipeline = root.Q<EnumField>("ABBuildPipeline");
            var ui_reference_resolution = root.Q<Vector2Field>("UIReferenceResolution");
            var output_path = root.Q<TextField>("BuildAppOutputPath");

            development_build.value = BuildApp.DevelopmentBuild;
            is_test_server.value = BuildApp.IsTestServer;
            load_assets_mold.Init(BuildApp.LoadAssetsMold);
            app_frame_rate.value = BuildApp.AppFrameRate.ToString();
            build_mold.Init(BuildApp.ApkTarget);
            export_project.value = BuildApp.NativeApp;
            ab_build_pipeline.Init(BuildApp.Pipeline);
            ui_reference_resolution.value = BuildApp.UIReferenceResolution;
            output_path.value = BuildApp.outputPath;

            development_build.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                BuildApp.DevelopmentBuild = evt.newValue;
            });
            is_test_server.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                BuildApp.IsTestServer = evt.newValue;
            });
            load_assets_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (LoadAssetsMold)System.Enum.Parse(typeof(LoadAssetsMold), evt.newValue);
                BuildApp.LoadAssetsMold = mold;
            });
            app_frame_rate.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                BuildApp.AppFrameRate = int.Parse(evt.newValue);
            });
            build_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (TargetPackage)System.Enum.Parse(typeof(TargetPackage), evt.newValue);
                export_project.style.display =
                    mold == TargetPackage.Mobile ? DisplayStyle.Flex : DisplayStyle.None;
                BuildApp.ApkTarget = mold;
            });
            export_project.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                BuildApp.NativeApp = evt.newValue;
            });
            ab_build_pipeline.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (ABPipeline)System.Enum.Parse(typeof(ABPipeline), evt.newValue);
                BuildApp.Pipeline = mold;
            });
            ui_reference_resolution.RegisterCallback<ChangeEvent<Vector2>>((evt) =>
            {
                BuildApp.UIReferenceResolution = evt.newValue;
            });
            output_path.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                BuildApp.outputPath = evt.newValue;
            });
            root.Q<Button>("BuildAppOutputPathBrowse").clicked += () =>
            {
                output_path.value = EditorTool.Browse(true);
                BuildApp.outputPath = output_path.value;
            };
            root.Q<Button>("BuildAppApply").clicked += () => { BuildApp.ApplyConfig(); };
            root.Q<Button>("BuildApp").clicked += () =>
            {
                BuildApp.ApplyConfig();
                BuildApp.Build();
            };
        }

        private void BuildAssetBundleFunction()
        {
            BuildAssetBundle.Init();
            var build_target = root.Q<EnumField>("BuildTarget");
            build_target.Init(BuildTarget.Android);

            var build_path = root.Q<TextField>("BuildPath");
            build_path.value = "Assets/Resources/HybridFolder";
            root.Q<Button>("BuildPath_Browse").clicked += () =>
            {
                build_path.value = EditorTool.Browse();
                BuildAssetBundle.buildPath = build_path.value;
            };
            var output_path = root.Q<TextField>("OutputPath");
            output_path.value = Application.dataPath.Replace("Assets", "AssetBundle");
            root.Q<Button>("OutputPath_Browse").clicked += () =>
            {
                output_path.value = EditorTool.Browse(true);
                BuildAssetBundle.outputPath = output_path.value;
            };
            var res_version = root.Q<TextField>("ResVersion");
            res_version.value = BuildAssetBundle.GetResVersion();
            res_version.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                BuildAssetBundle.SetResVersion(evt.newValue);
            });
            var update_des = root.Q<TextField>("UpdateDes");
            update_des.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                BuildAssetBundle.des = evt.newValue;
            });

            var folder_list = root.Q<ScrollView>("FolderList");
            var label_page = root.Q<Label>("ListText");

            RefreshAssetBundleList(folder_list, label_page);

            var ab_mold = root.Q<EnumField>("ABMold");
            ab_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (ABMold)System.Enum.Parse(typeof(ABMold), evt.newValue);
                BuildAssetBundle.SetBuildPath(mold);
                build_path.value = BuildAssetBundle.buildPath;
                RefreshAssetBundleList(folder_list, label_page);
                root.Q<Button>("BuildAndCopyDll").style.display =
                    mold == ABMold.Hybrid ? DisplayStyle.Flex : DisplayStyle.None;
            });
            ab_mold.Init(ABMold.Hybrid);
            ab_mold.value = ABMold.Hybrid;

            root.Q<Button>("All").clicked += () =>
            {
                BuildAssetBundle.SelectList(true);
                RefreshAssetBundleList(folder_list, label_page);
            };
            root.Q<Button>("NotAll").clicked += () =>
            {
                BuildAssetBundle.SelectList(false);
                RefreshAssetBundleList(folder_list, label_page);
            };

            root.Q<Button>("BuildAndCopyDll").clicked += () =>
            {
                BuildAssetBundle.BuildAndCopyDll((BuildTarget)build_target.value);
            };
            root.Q<Button>("AutoBuildAllAssetBuildBundle").clicked += () =>
            {
                BuildAssetBundle.AutoBuildAssetBundle((BuildTarget)build_target.value);
            };
            root.Q<Button>("DeleteAllAssetBundle").clicked += () =>
            {
                BuildAssetBundle.DeleteAssetBundle((BuildTarget)build_target.value);
            };
            root.Q<Button>("RemoveAllAssetsBundleLabels").clicked += () =>
            {
                BuildAssetBundle.RemoveAllAssetBundleLabels();
            };
            root.Q<Button>("SetAllAssetBundleLabels").clicked += () =>
            {
                BuildAssetBundle.SetAssetBundleLabels();
            };
            root.Q<Button>("BuildAllAssetBuildBundle").clicked += () =>
            {
                BuildAssetBundle.BuildAllAssetBundles((BuildTarget)build_target.value);
            };
            root.Q<Button>("CreateMD5File").clicked += () =>
            {
                BuildAssetBundle.CreateMD5File((BuildTarget)build_target.value);
            };
        }

        private void SetAppScriptConfigFunction()
        {
            var script_scroll_view = root.Q<ScrollView>("script_scroll_view");
            var script_list = SetAppScriptConfig.GetRootScripts();
            var main_scene_name = root.Q<DropdownField>("main_scene_name");
            main_scene_name.choices = SetAppScriptConfig.SceneNames;
            main_scene_name.value = SetAppScriptConfig.SceneNames[SetAppScriptConfig.level];
            main_scene_name.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                SetAppScriptConfig.level = SetAppScriptConfig.SceneNames.IndexOf(evt.newValue);
            });

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
            var table_mold = root.Q<EnumField>("table_mold");
            table_mold.Init(TableMold.Json);
            var table_path = root.Q<TextField>("table_path");
            table_path.value = $"{Application.dataPath.Replace("Assets", "")}Data/excel";
            root.Q<Button>("table_path_browse").clicked += () =>
            {
                table_path.value = EditorTool.Browse(true);
            };
            root.Q<Button>("table_apply").clicked += () =>
            {
                Table2CSharp.Apply(table_path.value, (TableMold)table_mold.value);
            };
        }

        private void ChangePrefabsFontFunction()
        {
            var textPrefabsField = root.Q<TextField>("text_prefab_path");
            textPrefabsField.value = "";
            var fontObjectField = root.Q<ObjectField>("object_font");
            fontObjectField.objectType = typeof(Font);

            root.Q<Button>("prefab_path_browse").clicked += () => { textPrefabsField.value = EditorTool.Browse(); };

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

            root.Q<Button>("files_path_browse").clicked += () => { textFilesField.value = EditorTool.Browse(); };

            root.Q<Button>("btn_find").clicked += () =>
            {
                FindSameFileName.FindSameFile((ObjectType)objectType.value, textFilesField.value);
            };
        }

        private void CopyTemplateScriptsFunction()
        {
            var unity_install_path = root.Q<TextField>("unity_install_path");
            unity_install_path.value = AppDomain.CurrentDomain.BaseDirectory;
            root.Q<Button>("unity_install_path_browse").clicked += () =>
            {
                unity_install_path.value = EditorTool.Browse(true);
            };
            root.Q<Button>("btn_copy_template").clicked += () =>
            {
                CopyTemplateScripts.Copy(unity_install_path.value);
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

                    table.Q<Button>("btn_table_remove").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabDeleted Icon").image);
                    table.Q<Button>("btn_table_add").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabCreate Icon").image);

                    table.Q<EnumField>("TableMold").Init(table_list[index].TableMold);
                    table.Q<EnumField>("TableMold").RegisterCallback<ChangeEvent<string>>((ent) =>
                    {
                        var mold = (TableMold)System.Enum.Parse(typeof(TableMold), ent.newValue);
                        SetAppTableConfig.SetConfigMoldValue(index, mold);
                    });
                    table.Q<TextField>("TableName").value = table_list[index].TableName;
                    table.Q<TextField>("TableName").RegisterCallback<ChangeEvent<string>>((ent) =>
                    {
                        SetAppTableConfig.SetConfigNameValue(index, ent.newValue);
                    });
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
                    script.Q<Button>("btn_script_remove").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabDeleted Icon").image);
                    script.Q<Button>("btn_script_add").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabCreate Icon").image);


                    var scenename = script.Q<DropdownField>("SceneName");
                    var scriptname = script.Q<DropdownField>("ScriptName");
                    
                    scenename.choices = SetAppScriptConfig.SceneNames;
                    scenename.value = SetAppScriptConfig.SceneNames[SetAppScriptConfig.SceneNames.IndexOf(script_list[index].SceneName)];
                    
                    scriptname.choices = SetAppScriptConfig.ScriptNames;
                    scriptname.value = SetAppScriptConfig.ScriptNames[SetAppScriptConfig.ScriptNames.IndexOf(script_list[index].ScriptName)];
                    
                    scenename.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        SetAppScriptConfig.SetConfigSceneValue(index, evt.newValue);
                        UpdateFoldoutText(script);
                    });
                    
                    scriptname.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        SetAppScriptConfig.SetConfigScriptValue(index, evt.newValue);
                        UpdateFoldoutText(script);
                    });

                    UpdateFoldoutText(script);
                    
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

        private void UpdateFoldoutText(VisualElement script)
        {
            var scenenames = script.Q<DropdownField>("SceneName").value.Split('/');
            var scriptnames = script.Q<DropdownField>("ScriptName").value.Split('.');
            var scene_label = scenenames[scenenames.Length - 1];
            var script_label = scriptnames[scriptnames.Length - 1];
            var label = $"LogicScript (SceneName:{scene_label}|ScriptName:{script_label})";
            script.Q<Foldout>("LogicScript").text = label;
        }

        private void RefreshAssetBundleList(ScrollView folder_list, Label label_page)
        {
            folder_list.Clear();
            for (int i = 0; i < BuildAssetBundle.m_DataList.Count; i++)
            {
                int index = i;
                Toggle toggle = new Toggle(BuildAssetBundle.m_DataList[index]);
                toggle.value = BuildAssetBundle.m_ExportList[index];
                toggle.RegisterCallback<ChangeEvent<bool>>((ent) =>
                {
                    BuildAssetBundle.m_ExportList[index] = ent.newValue;
                    label_page.text = $"打包 : {BuildAssetBundle.SelectCount()} / {BuildAssetBundle.m_ExportList.Count}";
                });
                folder_list.Add(toggle);
            }

            label_page.text = $"打包 : {BuildAssetBundle.SelectCount()} / {BuildAssetBundle.m_ExportList.Count}";
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
            var helpBox = new HelpBox(msg, HelpBoxMessageType.Info);
            infos.Add(helpBox);
        }
    }
}