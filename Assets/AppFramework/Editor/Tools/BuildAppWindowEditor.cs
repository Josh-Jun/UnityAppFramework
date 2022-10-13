using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR;
using UnityEngine;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
using System;
using Unity.VisualScripting;

public class BuildAppWindowEditor : EditorWindow
{
    private GUIStyle titleStyle;
    public AppConfig AppConfig;//App配置表 
    private readonly string configPath = "App/AppConfig";
    private bool DevelopmentBuild = true;
    private bool IsTestServer = true;
    private bool IsHotfix = false;
    private bool RunXLuaScripts = false;
    private int AppFrameRate = 30;
    private TargetPackage ApkTarget = TargetPackage.Mobile;
    private bool NativeApp = false;
    private ABPipeline Pipeline = ABPipeline.Default;
    private string outputPath;

    [MenuItem("Tools/My ToolsWindow/Build App", false, 0)]
    public static void OpenWindow()
    {
        BuildAppWindowEditor appWindow = GetWindow<BuildAppWindowEditor>("Build App");
        appWindow.Show();
    }
    public void OnEnable()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12,
        };

        outputPath = Application.dataPath.Replace("Assets", "App");
        AppConfig = Resources.Load<AppConfig>(configPath);
        if (AppConfig != null)
        {
            DevelopmentBuild = AppConfig.IsDebug;
            IsTestServer = AppConfig.IsTestServer;
            IsHotfix = AppConfig.IsHotfix;
            RunXLuaScripts = AppConfig.RunXLua;
            AppFrameRate = AppConfig.AppFrameRate;
            ApkTarget = AppConfig.TargetPackage;
            NativeApp = AppConfig.NativeApp;
            Pipeline = AppConfig.ABPipeline;
        }
        else
        {
            Debug.Log("找不到AppConfig，请新建AppConfig");
        }
    }
    public void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label(new GUIContent("Build App"), titleStyle);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Development Build"));
        GUILayout.FlexibleSpace();
        DevelopmentBuild = EditorGUILayout.Toggle(DevelopmentBuild, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Is Test Server"));
        GUILayout.FlexibleSpace();
        IsTestServer = EditorGUILayout.Toggle(IsTestServer, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Is Hotfix"));
        GUILayout.FlexibleSpace();
        IsHotfix = EditorGUILayout.Toggle(IsHotfix, GUILayout.MaxWidth(75));
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
        ApkTarget = (TargetPackage)EditorGUILayout.EnumPopup(ApkTarget, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();
        
        if (ApkTarget == TargetPackage.Mobile)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Export Project"));
            GUILayout.FlexibleSpace();
            NativeApp = EditorGUILayout.Toggle(NativeApp, GUILayout.MaxWidth(75));
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("AB Build Pipeline"));
        GUILayout.FlexibleSpace();
        Pipeline = (ABPipeline)EditorGUILayout.EnumPopup(Pipeline, GUILayout.MaxWidth(75));
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Apply"))
        {
            ApplyConfig();
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
            ApplyConfig();
            BuildApp();
        }
    }
    public void ApplyConfig()
    {
        AppConfig.IsDebug = DevelopmentBuild;
        AppConfig.IsHotfix = IsHotfix;
        AppConfig.IsTestServer = IsTestServer;
        AppConfig.RunXLua = RunXLuaScripts;
        AppConfig.AppFrameRate = AppFrameRate;
        AppConfig.TargetPackage = ApkTarget;
        AppConfig.NativeApp = NativeApp;
        AppConfig.ABPipeline = Pipeline;

        EditorUtility.SetDirty(AppConfig);

        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            // PlayerSettings.Android.keystorePass = "";
            // PlayerSettings.Android.keyaliasName = "";
            // PlayerSettings.Android.keyaliasPass = "";
            
            PlayerSettings.Android.minSdkVersion =ApkTarget == TargetPackage.Pico ? AndroidSdkVersions.AndroidApiLevel26 : AndroidSdkVersions.AndroidApiLevel23;
            
            EditorUserBuildSettings.exportAsGoogleAndroidProject = NativeApp;
#if PICO_XR_SETTING
            //此段代码依赖PicoXR的UPM 和 PicoTools的UPM
            XRGeneralSettings androidXRSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);

            if (androidXRSettings == null)
            {
                var assignedSettings = ScriptableObject.CreateInstance<XRManagerSettings>() as XRManagerSettings;
                androidXRSettings.AssignedSettings = assignedSettings;
                EditorUtility.SetDirty(androidXRSettings); // Make sure this gets picked up for serialization later.
            }

            //取消当前选择的
            IReadOnlyList<XRLoader> list = androidXRSettings.Manager.activeLoaders;
            int hasCount = list.Count;
            //Debug.Log(hasCount);
            for (int i = 0; i < hasCount; i++)
            {
                string nameTemp = list[0].GetType().FullName;
                Debug.Log("disable xr plug:" + nameTemp);
                XRPackageMetadataStore.RemoveLoader(androidXRSettings.Manager, nameTemp, BuildTargetGroup.Android);
            }
            androidXRSettings.InitManagerOnStart = false;
            if (ApkTarget == TargetPackage.Pico)
            {
                androidXRSettings.InitManagerOnStart = true;
                //启用
                string loaderTypeName = "Unity.XR.PXR.PXR_Loader";
                XRPackageMetadataStore.AssignLoader(androidXRSettings.Manager, loaderTypeName, BuildTargetGroup.Android);
            }
            EditorUtility.SetDirty(androidXRSettings); // Make sure this gets picked up for serialization later.
#endif
        }
        AssetDatabase.Refresh();
    }
    private string assetPath = "Assets/Resources/AssetsFolder";
    private string localAssetPath = "Assets/Resources/App";
    private void BuildApp()
    {
        string newPath = "";
        string newLocalPath = "";
        if (IsHotfix)
        {
            //移动到根目录
            newPath = MoveAssetsToRoot(assetPath);
            newLocalPath = MoveAssetsToRoot(localAssetPath);
        }
        string _str = ApkTarget == TargetPackage.Mobile ? "meta-app" : "meta-vr";
        string version = PlayerSettings.bundleVersion;
        string date = string.Format("{0}{1:00}{2:00}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        string name = DevelopmentBuild ? string.Format("{0}_v{1}_beta-{2}", _str, version, date) : string.Format("{0}_v{1}_release-{2}", _str, version, date);
        string suffix = NativeApp || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? "" : ".apk";
        string outName = string.Format("{0}{1}", name, suffix);
        EditorUserBuildSettings.SwitchActiveBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup, EditorUserBuildSettings.activeBuildTarget);
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
        var buildOption = BuildOptions.None;
        if (DevelopmentBuild)
        {
            buildOption |= BuildOptions.Development;
        }
        buildOption |= BuildOptions.CompressWithLz4;
        BuildPipeline.BuildPlayer(GetBuildScenes(), BuildPath, EditorUserBuildSettings.activeBuildTarget, buildOption);

        if (IsHotfix)
        {
            //移动回原始目录
            MoveAssetsToOriginPath(newPath, assetPath);
            MoveAssetsToOriginPath(newLocalPath, localAssetPath);
        }

        EditorUtility.RevealInFinder(BuildPath);

        Debug.Log(string.Format("GenericBuild Success Path = {0}", BuildPath));
    }
    string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null) continue;
            if (e.enabled)
            {
                if (IsHotfix)
                {
                    if (names.Count > 0)
                    {
                        break;
                    }
                }
                names.Add(e.path);
            }
        }
        return names.ToArray();
    }

    #region 资源移动
    /// <summary>
    /// 将资源移动到根目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string MoveAssetsToRoot(string path)
    {
        path = AbsolutePathToRelativePath(path);
        string floderName = path.Split('/').Last();
        EditorUtility.DisplayCancelableProgressBar("正在移动资源", "", 0);
        //移动资源
        string newPath = string.Format("Assets/{0}", floderName);
        AssetDatabase.MoveAsset(path, newPath);
        EditorUtility.ClearProgressBar();
        return RelativePathToAbsolutePath(newPath);
    }

    /// <summary>
    /// 将资源移动到原始目录
    /// </summary>
    /// <param name="currentPath"></param>
    /// <param name="originPath"></param>
    private static void MoveAssetsToOriginPath(string currentPath, string originPath)
    {
        currentPath = AbsolutePathToRelativePath(currentPath);
        originPath = AbsolutePathToRelativePath(originPath);
        EditorUtility.DisplayCancelableProgressBar("正在移动资源", "", 0);
        AssetDatabase.MoveAsset(currentPath, originPath);
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 相对路径转到绝对路径
    /// </summary>
    /// <param name="relativePath"></param>
    /// <returns></returns>
    private static string RelativePathToAbsolutePath(string relativePath)
    {
        return string.Format("{0}/{1}", Application.dataPath, relativePath.Replace("Assets/", ""));
    }

    /// <summary>
    /// 绝对路径转到相对路径
    /// </summary>
    /// <param name="absolutePath"></param>
    /// <returns></returns>
    private static string AbsolutePathToRelativePath(string absolutePath)
    {
        return absolutePath.Substring(absolutePath.IndexOf("Assets"));
    }
    #endregion
}
