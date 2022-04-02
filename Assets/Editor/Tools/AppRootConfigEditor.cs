using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;

public class AppRootConfigWindowEditor : EditorWindow
{
    private static GUIStyle titleStyle;
    private static RootScriptConfig config;
    private static readonly string configPath = "AssetsFolder/App/Assets/AppRootConfig";
    [MenuItem("Tools/My ToolsWindow/Set AppRootConfig", false, 2)]
    public static void OpenWindow()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12,
        };

        var bytes = Resources.Load<TextAsset>(configPath).bytes;
        config = XmlSerializeManager.ProtoDeSerialize<RootScriptConfig>(bytes);
        
        AppRootConfigWindowEditor window = GetWindow<AppRootConfigWindowEditor>("AppRootConfigWindow");
        window.Show();
    }
    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label(new GUIContent("AppRootConfig"), titleStyle);
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        if (config.RootScript.Count > 0)
        {
            for (int i = 0; i < config.RootScript.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(titleStyle);
                GUILayout.Label(new GUIContent("RootScript"), titleStyle);
                GUILayout.Label("1.SceneName");
                config.RootScript[i].SceneName = EditorGUILayout.TextField(config.RootScript[i].SceneName);
                GUILayout.Label("2.ScriptName");
                config.RootScript[i].ScriptName = EditorGUILayout.TextField(config.RootScript[i].ScriptName);
                GUILayout.Label("3.LuaScriptPath");
                config.RootScript[i].LuaScriptPath = EditorGUILayout.TextField(config.RootScript[i].LuaScriptPath);
                if (GUILayout.Button("Remove", titleStyle))
                {
                    if (config.RootScript.Count > 1)
                    {
                        Remove(i);
                        config.RootScript.RemoveAt(i);
                    }
                    else
                    {
                        Debuger.Log("不能删除最后一个RootScript");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Add New Root Script"))
        {
            RootScript rootScript = new RootScript
            {
                SceneName = "TestScene",
                ScriptName = "Test.TestRoot",
                LuaScriptPath = "Test/TestRoot",
            };
            Add(rootScript);
            config.RootScript.Add(rootScript);
        }
        if (GUILayout.Button("UpdateConfig"))
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
        var nodes = xmlDocument.GetElementsByTagName("RootScript");
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes[i];
            for (int j = 0; j < config.RootScript.Count; j++)
            {
                if(i == j)
                {
                    node.Attributes["SceneName"].Value = config.RootScript[j].SceneName;
                    node.Attributes["ScriptName"].Value = config.RootScript[j].ScriptName;
                    node.InnerText = config.RootScript[j].LuaScriptPath;
                }
            }
        }
        xmlDocument.Save(path);
    }
}
