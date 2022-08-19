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
    private string configPath = "AssetsFolder/App/Config/AppTableConfig";
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

        config = Resources.Load<AppTableConfig>(configPath);
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
                        TablePath = "Table/Xml/TestTableData",
                    };
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
    public void ApplyConfig()
    {
        EditorUtility.SetDirty(config);
        AssetDatabase.Refresh();
    }
}
