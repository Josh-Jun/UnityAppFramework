using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;

public class AppTableConfigWindowEditor : EditorWindow
{
    private GUIStyle titleStyle;
    private AppTableConfig config;
    private string configPath = "AssetsFolder/App/Assets/AppTableConfig";
    [MenuItem("Tools/My ToolsWindow/Set AppTableConfig", false, 2)]
    public static void OpenWindow()
    {
        AppTableConfigWindowEditor window = GetWindow<AppTableConfigWindowEditor>("AppTableConfigWindow");
        window.Show();
    }
    public void OnEnable()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12,
        };

        var bytes = Resources.Load<TextAsset>(configPath).bytes;
        config = XmlManager.ProtoDeSerialize<AppTableConfig>(bytes);
    }
    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label(new GUIContent("AppTableConfig"), titleStyle);
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        if (config.AppTable.Count > 0)
        {
            for (int i = 0; i < config.AppTable.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(titleStyle);
                GUILayout.Label(new GUIContent("AppTable"), titleStyle);
                GUILayout.Label("1.TableName");
                config.AppTable[i].TableName = EditorGUILayout.TextField(config.AppTable[i].TableName);
                GUILayout.Label("2.TablePath");
                config.AppTable[i].TablePath = EditorGUILayout.TextField(config.AppTable[i].TablePath);
                if (GUILayout.Button("", new GUIStyle("OL Minus")))
                {
                    if (config.AppTable.Count > 1)
                    {
                        Remove(i);
                        config.AppTable.RemoveAt(i);
                    }
                    else
                    {
                        Debug.Log("不能删除最后一个Table");
                    }
                }
                //OL Minus OL Plus
                if (GUILayout.Button("", new GUIStyle("OL Plus")))
                {
                    AppTable appTable = new AppTable
                    {
                        TableName = "TestTableData",
                        TablePath = "Table/Test/TestTableData",
                    };
                    Add(appTable);
                    config.AppTable.Add(appTable);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Apply"))
        {
            ApplyConfig();
        }
        EditorGUILayout.EndVertical();
    }
    public void Add(AppTable appTable)
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var root = xmlDocument.DocumentElement;
        var script = xmlDocument.CreateElement("AppTable");
        script.SetAttribute("TableName", appTable.TableName);
        script.InnerText = appTable.TablePath;
        root.AppendChild(script);
        xmlDocument.Save(path);
    }
    public void Remove(int id)
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var nodes = xmlDocument.GetElementsByTagName("AppTable");
        XmlNode node = nodes[id];
        node.ParentNode.RemoveChild(node);
        xmlDocument.Save(path);
    }
    public void ApplyConfig()
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var nodes = xmlDocument.GetElementsByTagName("AppTable");
        for (int i = 0; i < nodes.Count; i++)
        {
            XmlNode node = nodes[i];
            for (int j = 0; j < config.AppTable.Count; j++)
            {
                if(i == j)
                {
                    node.Attributes["TableName"].Value = config.AppTable[j].TableName;
                    node.InnerText = config.AppTable[j].TablePath;
                }
            }
        }
        xmlDocument.Save(path);
        AssetDatabase.Refresh();
    }
}
