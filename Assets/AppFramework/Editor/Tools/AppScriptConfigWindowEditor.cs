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
    private static readonly string configPath = "AssetsFolder/App/Assets/AppScriptConfig";
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

        var bytes = Resources.Load<TextAsset>(configPath).bytes;
        config = XmlManager.ProtoDeSerialize<AppScriptConfig>(bytes);
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
                GUILayout.Label("2.ScriptName");
                config.RootScript[i].ScriptName = EditorGUILayout.TextField(config.RootScript[i].ScriptName);
                GUILayout.Label("3.LuaScriptPath");
                config.RootScript[i].LuaScriptPath = EditorGUILayout.TextField(config.RootScript[i].LuaScriptPath);
                if (GUILayout.Button("", new GUIStyle("OL Minus")))
                {
                    if (config.RootScript.Count > 1)
                    {
                        Remove(i);
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
                        SceneName = "TestScene",
                        ScriptName = "Test.TestRoot",
                        LuaScriptPath = "Test/TestRoot",
                    };
                    int index = SceneNames.IndexOf("TestScene");
                    Add(rootScript);
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
    public void Add(RootScript rootScript)
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var root = xmlDocument.DocumentElement;
        var script = xmlDocument.CreateElement("RootScript");
        script.SetAttribute("SceneName", rootScript.SceneName);
        script.SetAttribute("ScriptName", rootScript.ScriptName);
        script.InnerText = rootScript.LuaScriptPath;
        root.AppendChild(script);
        xmlDocument.Save(path);
    }
    public void Remove(int id)
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var nodes = xmlDocument.GetElementsByTagName("RootScript");
        XmlNode node = nodes[id];
        node.ParentNode.RemoveChild(node);
        xmlDocument.Save(path);
    }
    public void UpdateConfig()
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        xmlDocument.DocumentElement.SetAttribute("MainSceneName", SceneNames[level]);
        var nodes = xmlDocument.GetElementsByTagName("RootScript");
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes[i];
            for (int j = 0; j < config.RootScript.Count; j++)
            {
                if (i == j)
                {
                    node.Attributes["SceneName"].Value = SceneNames[levels[j]];
                    node.Attributes["ScriptName"].Value = config.RootScript[j].ScriptName;
                    node.InnerText = config.RootScript[j].LuaScriptPath;
                }
            }
        }
        xmlDocument.Save(path);
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
