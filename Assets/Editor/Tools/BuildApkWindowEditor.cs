using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class BuildApkWindowEditor : EditorWindow
{
    private static GUIStyle titleStyle;
    public static AppConfig AppConfig;//App配置表 
    private static readonly string configPath = "App/AppConfig";
    private bool DevelopmentBuild = true;
    private bool IsProServer = true;
    private bool IsHotfix = false;
    private bool IsLoadAB = false;
    private bool RunXLuaScripts = false;
    private int AppFrameRate = 30;
    private ApkTarget ApkTarget = ApkTarget.Android;
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
        AppConfig = Resources.Load<AppConfig>(configPath);
        if (AppConfig != null)
        {
            DevelopmentBuild = AppConfig.IsDebug;
            IsProServer = AppConfig.IsProServer;
            IsHotfix = AppConfig.IsHotfix;
            IsLoadAB = AppConfig.IsLoadAB;
            RunXLuaScripts = AppConfig.RunXLua;
            AppFrameRate = AppConfig.AppFrameRate;
            ApkTarget = AppConfig.ApkTarget;
        }
        else
        {
            Debug.Log("找不到AppConfig，请新建AppConfig");
        }
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
        GUILayout.Label(new GUIContent("Is Pro Server"));
        GUILayout.FlexibleSpace();
        IsProServer = EditorGUILayout.Toggle(IsProServer, GUILayout.MaxWidth(75));
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
        GUILayout.Label(new GUIContent("App FrameRate"));
        GUILayout.FlexibleSpace();
        AppFrameRate = EditorGUILayout.IntField(AppFrameRate, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Build Mold"));
        GUILayout.FlexibleSpace();
        ApkTarget = (ApkTarget)EditorGUILayout.EnumPopup(ApkTarget, GUILayout.MaxWidth(75));
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
        EditorUserBuildSettings.development = DevelopmentBuild;
        AppConfig.IsDebug = DevelopmentBuild;
        AppConfig.IsHotfix = IsHotfix;
        AppConfig.IsLoadAB = IsLoadAB;
        AppConfig.IsProServer = IsProServer;
        AppConfig.RunXLua = RunXLuaScripts;
        AppConfig.AppFrameRate = AppFrameRate;
        AppConfig.ApkTarget = ApkTarget;
    }
    private void BuildApk()
    {
        string _str = ApkTarget == ApkTarget.PicoVR ? "meta-picovr" : "meta-android";
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
