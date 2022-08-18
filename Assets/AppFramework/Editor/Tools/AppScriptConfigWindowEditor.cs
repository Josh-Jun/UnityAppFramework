using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;

public class AppScriptConfigWindowEditor : EditorWindow
{
    private GUIStyle titleStyle;
    private AppScriptConfig config;
    private string MainSceneName = "Test";
    private int level = 0;
    private List<string> SceneNames = new List<string>();
    private List<int> levels = new List<int>();
    private static readonly string configPath = "AssetsFolder/App/Config/AppScriptConfig";
    [MenuItem("Tools/My ToolsWindow/Set AppScriptConfig", false, 2)]
    public static void OpenWindow()
    {
        AppScriptConfigWindowEditor window = GetWindow<AppScriptConfigWindowEditor>("AppScriptConfigWindow");
        window.Show();
    }
    private void OnEnable()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12,
        };

        config = Resources.Load<AppScriptConfig>(configPath);
        MainSceneName = config.MainSceneName;

        SceneNames.Add("Global");
        for (var i = 0; i < GetBuildScenes().Length; i++)
        {
            var dic = Path.GetDirectoryName(GetBuildScenes()[i]);
            var name = Path.GetFileNameWithoutExtension(GetBuildScenes()[i]);
            var fullName = Path.Combine(dic, name).Replace(@"\","/");
            var scene = "";
            if (i > 0)
            {
                scene = fullName.Replace("Assets/Resources/AssetsFolder/", "");
            }
            SceneNames.Add(scene);
        }

        level = SceneNames.IndexOf(MainSceneName);

        for (int i = 0; i < config.RootScript.Count; i++)
        {
            int index = SceneNames.IndexOf(config.RootScript[i].SceneName);
            levels.Add(index);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label(new GUIContent("AppScriptConfig"), titleStyle);
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("MainSceneName"), GUILayout.MaxWidth(100));
        level = EditorGUILayout.Popup(level, SceneNames.ToArray(), GUILayout.MaxWidth(150));
        config.MainSceneName = SceneNames[level];
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (config.RootScript.Count > 0)
        {
            for (int i = 0; i < config.RootScript.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(titleStyle);
                GUILayout.Label(new GUIContent("RootScript"), titleStyle);
                GUILayout.Label("1.SceneName");
                levels[i] = EditorGUILayout.Popup(levels[i],SceneNames.ToArray(), GUILayout.MaxWidth(180));
                config.RootScript[i].SceneName = SceneNames[levels[i]];
                GUILayout.Label("2.ScriptName");
                config.RootScript[i].ScriptName = EditorGUILayout.TextField(config.RootScript[i].ScriptName);
                GUILayout.Label("3.LuaScriptPath");
                config.RootScript[i].LuaScriptPath = EditorGUILayout.TextField(config.RootScript[i].LuaScriptPath);
                if (GUILayout.Button("", new GUIStyle("OL Minus")))
                {
                    if (config.RootScript.Count > 1)
                    {
                        config.RootScript.RemoveAt(i);
                        levels.RemoveAt(i);
                    }
                    else
                    {
                        Debug.Log("不能删除最后一个RootScript");
                    }
                }
                //OL Minus OL Plus
                if (GUILayout.Button("", new GUIStyle("OL Plus")))
                {
                    RootScript rootScript = new RootScript
                    {
                        SceneName = SceneNames[levels[levels.Count -1]],
                        ScriptName = "Test.TestRoot",
                        LuaScriptPath = "Test/TestRoot",
                    };
                    int index = SceneNames.IndexOf(rootScript.SceneName);
                    config.RootScript.Add(rootScript);
                    levels.Add(index);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Apply"))
        {
            UpdateConfig();
        }
        EditorGUILayout.EndVertical();
    }
    public void UpdateConfig()
    {
        foreach (int id in levels)
        {
            config.RootScript[id].SceneName = SceneNames[id];
        }
        EditorUtility.SetDirty(config);
        AssetDatabase.Refresh();
    }
    private string[] GetBuildScenes()
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
