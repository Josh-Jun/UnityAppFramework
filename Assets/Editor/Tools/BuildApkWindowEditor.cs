using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class BuildApkWindowEditor : EditorWindow
{
    private static GUIStyle titleStyle;
    private static readonly string configPath = "App/AppConfig";
    private bool DevelopmentBuild = true;
    private bool IsBetaServer = true;
    private bool IsHotfix = false;
    private bool IsLoadAB = false;
    private bool RunXLuaScripts = false;
    private AppTarget Target = AppTarget.Android;
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
        GUILayout.Label(new GUIContent("Is Hotfix"));
        GUILayout.FlexibleSpace();
        IsHotfix = EditorGUILayout.Toggle(IsHotfix, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Is LoadAB"));
        GUILayout.FlexibleSpace();
        IsLoadAB = EditorGUILayout.Toggle(IsLoadAB, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Run XLuaScripts"));
        GUILayout.FlexibleSpace();
        RunXLuaScripts = EditorGUILayout.Toggle(RunXLuaScripts, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Build Mold"));
        GUILayout.FlexibleSpace();
        Target = (AppTarget)EditorGUILayout.EnumPopup(Target, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Update"))
        {
            UpdateConfig();
        }

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
        XmlDocument xml_doc = new XmlDocument();
        xml_doc.Load(path);
        xml_doc.GetElementsByTagName("Debug")[0].InnerText = DevelopmentBuild.ToString().ToLower();
        xml_doc.GetElementsByTagName("Server")[0].InnerText = IsBetaServer.ToString().ToLower();
        xml_doc.GetElementsByTagName("Hotfix")[0].InnerText = IsHotfix.ToString().ToLower();
        xml_doc.GetElementsByTagName("LoadAB")[0].InnerText = IsLoadAB.ToString().ToLower();
        xml_doc.GetElementsByTagName("XLua")[0].InnerText = RunXLuaScripts.ToString().ToLower();
        xml_doc.Save(path);
    }
    private void BuildApk()
    {
        string _str = Target == AppTarget.PicoVR ? "meta-picovr" : "meta-android";
        string version = PlayerSettings.bundleVersion;
        string name = DevelopmentBuild ? string.Format("{0}_v{1}_beta", _str, version) : string.Format("{0}_v{1}_release", _str, version);
        string outName = string.Format("{0}.apk", name);
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, UnityEditor.BuildTarget.Android);
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
        BuildPipeline.BuildPlayer(GetBuildScenes(), BuildPath, UnityEditor.BuildTarget.Android, BuildOptions.None);

        EditorUtility.RevealInFinder(BuildPath);

        Debug.Log(string.Format("GenericBuild Success Path = {0}", BuildPath));
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
