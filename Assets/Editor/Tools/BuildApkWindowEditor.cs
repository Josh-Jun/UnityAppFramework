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
    private bool IsBetaServer = true;
    private bool IsPicoVR = false;
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

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Development Build"));
        GUILayout.FlexibleSpace();
        DevelopmentBuild = EditorGUILayout.Toggle(DevelopmentBuild, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Is Beta Server"));
        GUILayout.FlexibleSpace();
        IsBetaServer = EditorGUILayout.Toggle(IsBetaServer, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Is PicoVR"));
        GUILayout.FlexibleSpace();
        IsPicoVR = EditorGUILayout.Toggle(IsPicoVR, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            outputPath = EditorUtility.OpenFolderPanel("Bundle Folder", outputPath, string.Empty);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Build"))
        {
            UpdateConfig();
            BuildApk();
        }
    }
    public void UpdateConfig()
    {
        string path = string.Format(@"{0}/Resources/{1}.xml", Application.dataPath, configPath);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path);
        var root = xmlDocument.DocumentElement;
        root.Attributes["IsDebug"].Value = DevelopmentBuild.ToString();
        root.Attributes["IsBetaServer"].Value = IsBetaServer.ToString();
        xmlDocument.Save(path);
    }
    private void BuildApk()
    {
        string _str = IsPicoVR ? "meta-picovr" : "meta-android";
        string version = PlayerSettings.bundleVersion;
        string name = DevelopmentBuild ? string.Format("{0}_v{1}_beta", _str, version) : string.Format("{0}_v{1}_release", _str, version);
        string outName = string.Format("{0}.apk", name);
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        if (Directory.Exists(outputPath))
        {
            if (File.Exists(outName))
            {
                File.Delete(outName);
            }
        }
        else
        {
            Directory.CreateDirectory(outputPath);
        }
        string BuildPath = string.Format("{0}/{1}", outputPath, outName);
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
