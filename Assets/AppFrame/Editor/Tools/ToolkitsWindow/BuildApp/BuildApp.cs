using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppFrame.Config;
using AppFrame.Enum;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;
#if PICO_XR_SETTING
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
#endif

namespace AppFrame.Editor
{
    public class BuildApp
    {
        private static AppConfig AppConfig; //App配置表 
        private static string configPath = "AppConfig";
        public static bool DevelopmentBuild = true;
        public static bool IsTestServer = true;
        public static LoadAssetsMold LoadAssetsMold = LoadAssetsMold.Native;
        public static int AppFrameRate = 30;
        public static TargetPackage ApkTarget = TargetPackage.Mobile;
        public static bool NativeApp = false;
        public static ABPipeline Pipeline = ABPipeline.Default;
        public static string outputPath;

        public static void Init()
        {
            outputPath = Application.dataPath.Replace("Assets", "App");
            AppConfig = Resources.Load<AppConfig>(configPath);
            if (AppConfig != null)
            {
                DevelopmentBuild = AppConfig.IsDebug;
                IsTestServer = AppConfig.IsTestServer;
                LoadAssetsMold = AppConfig.LoadAssetsMold;
                AppFrameRate = AppConfig.AppFrameRate;
                ApkTarget = AppConfig.TargetPackage;
                NativeApp = AppConfig.NativeApp;
                Pipeline = AppConfig.ABPipeline;
            }
            else
            {
                ToolkitsWindow.ShowHelpBox("找不到AppConfig，请新建AppConfig");
            }
        }
        public static void ApplyConfig()
        {
            AppConfig.IsDebug = DevelopmentBuild;
            AppConfig.LoadAssetsMold = LoadAssetsMold;
            AppConfig.IsTestServer = IsTestServer;
            AppConfig.AppFrameRate = AppFrameRate;
            AppConfig.TargetPackage = ApkTarget;
            AppConfig.NativeApp = NativeApp;
            AppConfig.ABPipeline = Pipeline;

            EditorUtility.SetDirty(AppConfig);

            HybridCLRSettings.Instance.enable = LoadAssetsMold != LoadAssetsMold.Local;

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // PlayerSettings.Android.keystorePass = "";
                // PlayerSettings.Android.keyaliasName = "";
                // PlayerSettings.Android.keyaliasPass = "";

                PlayerSettings.Android.minSdkVersion = ApkTarget == TargetPackage.XR
                    ? AndroidSdkVersions.AndroidApiLevel26
                    : AndroidSdkVersions.AndroidApiLevel23;

                EditorUserBuildSettings.exportAsGoogleAndroidProject = NativeApp;
#if PICO_XR_SETTING
                //此段代码依赖PicoXR的UPM 和 PicoTools的UPM
                XRGeneralSettings androidXRSettings =
                    XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);

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
                if (ApkTarget == TargetPackage.XR)
                {
                    androidXRSettings.InitManagerOnStart = true;
                    //启用
                    string loaderTypeName = "Unity.XR.PXR.PXR_Loader";
                    XRPackageMetadataStore.AssignLoader(androidXRSettings.Manager, loaderTypeName,
                        BuildTargetGroup.Android);
                }

                EditorUtility.SetDirty(androidXRSettings); // Make sure this gets picked up for serialization later.
#endif
            }

            AssetDatabase.Refresh();
        }

        private static string assetsPath = "Assets/Resources/AssetsFolder";
        private static string hybridPath = "Assets/Resources/HybridFolder";

        public static void Build()
        {
            string newAssetsPath = "";
            string newHybridPath = "";
            if (LoadAssetsMold != LoadAssetsMold.Native)
            {
                //移动到根目录
                newAssetsPath = MoveAssetsToRoot(assetsPath);
                newHybridPath = MoveAssetsToRoot(hybridPath);
            }

            string str = ApkTarget == TargetPackage.Mobile ? "app" : "vr";
            string version = PlayerSettings.bundleVersion;
            string date = $"{DateTime.Now.Year}{DateTime.Now.Month:00}{DateTime.Now.Day:00}";
            string dev = DevelopmentBuild ? "beta" : "release";
            string suffix = NativeApp || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? "" : ".apk";
            string name = $"{PlayerSettings.applicationIdentifier.ToLower()}_{str}_v{version}_{dev}_{date}{suffix}";
            
            EditorUserBuildSettings.SwitchActiveBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup,
                EditorUserBuildSettings.activeBuildTarget);
            if (Directory.Exists(outputPath))
            {
                if (File.Exists(name))
                {
                    File.Delete(name);
                }
            }
            else
            {
                Directory.CreateDirectory(outputPath);
            }

            string BuildPath = string.Format("{0}/{1}", outputPath, name);
            var buildOption = BuildOptions.None;
            if (DevelopmentBuild)
            {
                buildOption |= BuildOptions.Development;
            }

            buildOption |= BuildOptions.CompressWithLz4;
            BuildPipeline.BuildPlayer(GetBuildScenes(), BuildPath, EditorUserBuildSettings.activeBuildTarget,
                buildOption);

            if (LoadAssetsMold != LoadAssetsMold.Native)
            {
                //移动回原始目录
                MoveAssetsToOriginPath(newAssetsPath, assetsPath);
                MoveAssetsToOriginPath(newHybridPath, hybridPath);
            }

            EditorUtility.RevealInFinder(BuildPath);

            Debug.Log(string.Format("GenericBuild Success Path = {0}", BuildPath));
        }

        private static string[] GetBuildScenes()
        {
            List<string> names = new List<string>();
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e == null) continue;
                if (e.enabled)
                {
                    if (LoadAssetsMold != LoadAssetsMold.Native)
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
}
