using System.Collections;
using System.Collections.Generic;
using System.IO;
using AppFrame.Config;
using UnityEditor;
using UnityEngine;

namespace AppFrame.Editor
{
    public class SetAppScriptConfig
    {
        private static AppScriptConfig config = null;
        private const string configPath = "HybridFolder/App/Config/AppScriptConfig";
        private static string MainSceneName = "Test";
        public static List<string> SceneNames = new List<string>();
        private static List<int> levels = new List<int>();
        public static int level = 0;

        public static List<LogicScript> GetRootScripts()
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

            return config.LogicScript;
        }

        public static void Remove(int index)
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
        
        public static void Add()
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

        public static void SetConfigSceneValue(int index, string value)
        {
            config.LogicScript[index].SceneName = value;
        }
        public static void SetConfigScriptValue(int index, string value)
        {
            config.LogicScript[index].ScriptName = value;
        }

        public static void ApplyConfig()
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