using System.Xml;
using UnityEditor;
using UnityEngine;

public class BuildApkWindowEditor : EditorWindow
{
    private static GUIStyle titleStyle;
    private static readonly string configPath = "AssetsFolder/App/Assets/AppRootConfig";
    private bool DevelopmentBuild = true;
    private string outputPath;

    [MenuItem("Tools/My ToolsWindow/Build Apk", false, 0)]
    public static void OpenWindow()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12,
        };

        BuildApkWindowEditor window = GetWindow<BuildApkWindowEditor>("Build Apk");
        window.Show();
    }
    public void OnEnable()
    {
        outputPath = Application.dataPath.Replace("Assets", "Apk");
    }
    public void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label(new GUIContent("Build Apk"), titleStyle);
        EditorGUILayout.Space();

        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            outputPath = EditorUtility.OpenFolderPanel("Bundle Folder", outputPath, string.Empty);
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Development Build"));
        GUILayout.FlexibleSpace();
        DevelopmentBuild = EditorGUILayout.Toggle(DevelopmentBuild, GUILayout.MaxWidth(100f));
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Build"))
        {
            UpdateConfig(DevelopmentBuild); 
            BuildApk();
        }
    }
    public void UpdateConfig(bool value)
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var root = xmlDocument.DocumentElement;
        root.Attributes["IsDebug"].Value = value.ToString();
        xmlDocument.Save(path);
    }
    private void BuildApk()
    {
        string outPath = string.Format("{0}/{1}", outputPath, "");
    }
}
