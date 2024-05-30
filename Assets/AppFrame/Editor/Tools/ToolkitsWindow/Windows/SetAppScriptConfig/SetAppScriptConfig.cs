using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using AppFrame.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public class SetAppScriptConfig : IToolkitEditor
    {
        private AppScriptConfig config = null;
        private const string configPath = "HybridFolder/App/Config/AppScriptConfig";
        private string MainSceneName = "Test";
        private List<string> SceneNames = new List<string>();
        private List<string> ScriptNames = new List<string>();
        private List<int> levels = new List<int>();
        private int level = 0;
        public void OnCreate(VisualElement root)
        {
            var script_scroll_view = root.Q<ScrollView>("script_scroll_view");
            var script_list = GetRootScripts();
            var main_scene_name = root.Q<DropdownField>("main_scene_name");
            main_scene_name.choices = SceneNames;
            main_scene_name.value = SceneNames[level];
            main_scene_name.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                level = SceneNames.IndexOf(evt.newValue);
            });

            RefreshScriptView(script_scroll_view);
            root.Q<Button>("btn_script_apply").clicked += ApplyConfig;
        }
        
        public void OnUpdate()
        {
            
        }
        public void OnDestroy()
        {
            
        }

        private void RefreshScriptView(ScrollView table_script_view)
        {
            var script_list = GetRootScripts();

            if (script_list.Count > 0)
            {
                table_script_view.Clear();
                for (int i = 0; i < script_list.Count; i++)
                {
                    int index = i;
                    var scriptItem = EditorTool.GetEditorWindowsAsset<VisualTreeAsset>($"SetAppScriptConfig/script_item.uxml");
                    VisualElement script = scriptItem.CloneTree();
                    script.Q<Button>("btn_script_remove").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabDeleted Icon").image);
                    script.Q<Button>("btn_script_add").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabCreate Icon").image);


                    var scenename = script.Q<DropdownField>("SceneName");
                    var scriptname = script.Q<DropdownField>("ScriptName");
                    
                    scenename.choices = SceneNames;
                    scenename.value = SceneNames[SceneNames.IndexOf(script_list[index].SceneName)];
                    
                    scriptname.choices = ScriptNames;
                    scriptname.value = ScriptNames[ScriptNames.IndexOf(script_list[index].ScriptName)];
                    
                    scenename.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        SetConfigSceneValue(index, evt.newValue);
                        UpdateFoldoutText(script);
                    });
                    
                    scriptname.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        SetConfigScriptValue(index, evt.newValue);
                        UpdateFoldoutText(script);
                    });

                    UpdateFoldoutText(script);
                    
                    script.Q<Button>("btn_script_remove").clicked += () =>
                    {
                        Remove(index);
                        RefreshScriptView(table_script_view);
                    };
                    script.Q<Button>("btn_script_add").clicked += () =>
                    {
                        Add();
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
        
        private List<LogicScript> GetRootScripts()
        {
            config = Resources.Load<AppScriptConfig>(configPath);
            MainSceneName = config.MainSceneName;
            SceneNames.Clear();
            SceneNames.Add("Global");
            for (var i = 0; i < GetBuildScenes().Length; i++)
            {
                var dic = Path.GetDirectoryName(GetBuildScenes()[i]);
                var name = Path.GetFileNameWithoutExtension(GetBuildScenes()[i]);
                var fullName = Path.Combine(dic, name).Replace(@"\", "/");
                var scene = "";
                if (i > 0)
                {
                    scene = fullName.Replace("Assets/Resources/AssetsFolder/", "");
                    scene = scene.Replace("Assets/Resources/HybridFolder/", "");
                }

                if (!SceneNames.Contains(scene))
                {
                    SceneNames.Add(scene);
                }
            }

            level = SceneNames.IndexOf(MainSceneName);

            for (int i = 0; i < config.LogicScript.Count; i++)
            {
                int index = SceneNames.IndexOf(config.LogicScript[i].SceneName);
                levels.Add(index);
            }
            
            ScriptNames = EditorTool.GetScriptName("App.Module","ILogic");

            return config.LogicScript;
        }

        private void Remove(int index)
        {
            if (config.LogicScript.Count > 1)
            {
                config.LogicScript.RemoveAt(index);
                levels.RemoveAt(index);
            }
            else
            {
                ToolkitsWindow.ShowHelpBox("不能删除最后一个RootScript");
            }
        }
        
        private void Add()
        {
            LogicScript logicScript = new LogicScript
            {
                SceneName = SceneNames[levels[levels.Count - 1]],
                ScriptName = "Modules.Test.TestLogic",
            };
            int index = SceneNames.IndexOf(logicScript.SceneName);
            config.LogicScript.Add(logicScript);
            levels.Add(index);
        }

        private void SetConfigSceneValue(int index, string value)
        {
            config.LogicScript[index].SceneName = value;
        }
        private void SetConfigScriptValue(int index, string value)
        {
            config.LogicScript[index].ScriptName = value;
        }

        private void ApplyConfig()
        {
            foreach (int id in levels)
            {
                if (id < config.LogicScript.Count && id < SceneNames.Count)
                {
                    config.LogicScript[id].SceneName = SceneNames[id];
                }
            }
            config.MainSceneName = SceneNames[level];
            EditorUtility.SetDirty(config);
            AssetDatabase.Refresh();
        }

        private static string[] GetBuildScenes()
        {
            List<string> names = new List<string>();
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e == null) continue;
                names.Add(e.path);
            }

            return names.ToArray();
        }
    }
}