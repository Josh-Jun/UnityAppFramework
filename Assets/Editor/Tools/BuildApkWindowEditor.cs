using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class BuildApkWindowEditor : EditorWindow
{
    private static GUIStyle titleStyle;
    private static readonly string configPath = "App/Debug/DebugConfig";
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
        DevelopmentBuild = EditorGUILayout.Toggle(DevelopmentBuild, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Build"))
        {
            UpdateConfig(DevelopmentBuild);
            //BuildApk();
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
        string name = DevelopmentBuild ? string.Format("{0}v{1}", "meta-beta", PlayerSettings.bundleVersion) : string.Format("{0}v{1}", "meta-pro", PlayerSettings.bundleVersion);
        string outName = string.Format("{0}.apk", name);
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        if (Directory.Exists(outPath))
        {
            if (File.Exists(outName))
            {
                File.Delete(outName);
            }
        }
        else
        {
            Directory.CreateDirectory(outPath);
        }
        string BuildPath = string.Format("{0}/{1}.apk", outPath, outName);
        BuildPipeline.BuildPlayer(GetBuildScenes(), BuildPath, BuildTarget.Android, BuildOptions.None);

        EditorUtility.DisplayDialog("Finish", string.Format("OutPut: {0}", BuildPath), "OK");

        var log = string.Format("GenericBuild Success Path = {0}", BuildPath);
        Debug.Log(log);
    }
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }
}
