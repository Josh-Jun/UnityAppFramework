using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppFrame.Config;
using AppFrame.Enum;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if PICO_XR_SETTING
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
#endif

namespace AppFrame.Editor
{
    public class BuildApp : IToolkitEditor
    {
        private AppConfig AppConfig; //App配置表 
        private string configPath = "AppConfig";
        private bool DevelopmentBuild = true;
        private bool IsTestServer = true;
        private LoadAssetsMold LoadAssetsMold = LoadAssetsMold.Native;
        private int AppFrameRate = 30;
        private TargetPackage ApkTarget = TargetPackage.Mobile;
        private bool NativeApp = false;
        private ABPipeline Pipeline = ABPipeline.Default;
        private Vector2 UIReferenceResolution;
        private string outputPath;

        public void OnCreate(VisualElement root)
        {
            Init();
            var development_build = root.Q<Toggle>("DevelopmentBuild");
            var is_test_server = root.Q<Toggle>("IsTestServer");
            var load_assets_mold = root.Q<EnumField>("LoadAssetsMold");
            var app_frame_rate = root.Q<TextField>("AppFrameRate");
            var build_mold = root.Q<EnumField>("BuildMold");
            var export_project = root.Q<Toggle>("ExportProject");
            var ab_build_pipeline = root.Q<EnumField>("ABBuildPipeline");
            var ui_reference_resolution = root.Q<Vector2Field>("UIReferenceResolution");
            var output_path = root.Q<TextField>("BuildAppOutputPath");

            development_build.value = DevelopmentBuild;
            is_test_server.value = IsTestServer;
            load_assets_mold.Init(LoadAssetsMold);
            app_frame_rate.value = AppFrameRate.ToString();
            build_mold.Init(ApkTarget);
            export_project.value = NativeApp;
            ab_build_pipeline.Init(Pipeline);
            ui_reference_resolution.value = UIReferenceResolution;
            output_path.value = outputPath;

            development_build.RegisterCallback<ChangeEvent<bool>>((evt) => { DevelopmentBuild = evt.newValue; });
            is_test_server.RegisterCallback<ChangeEvent<bool>>((evt) => { IsTestServer = evt.newValue; });
            load_assets_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (LoadAssetsMold)System.Enum.Parse(typeof(LoadAssetsMold), evt.newValue);
                LoadAssetsMold = mold;
            });
            app_frame_rate.RegisterCallback<ChangeEvent<string>>((evt) => { AppFrameRate = int.Parse(evt.newValue); });
            build_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (TargetPackage)System.Enum.Parse(typeof(TargetPackage), evt.newValue);
                export_project.style.display =
                    mold == TargetPackage.Mobile ? DisplayStyle.Flex : DisplayStyle.None;
                ApkTarget = mold;
            });
            export_project.RegisterCallback<ChangeEvent<bool>>((evt) => { NativeApp = evt.newValue; });
            ab_build_pipeline.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (ABPipeline)System.Enum.Parse(typeof(ABPipeline), evt.newValue);
                Pipeline = mold;
            });
            ui_reference_resolution.RegisterCallback<ChangeEvent<Vector2>>((evt) =>
            {
                UIReferenceResolution = evt.newValue;
            });
            output_path.RegisterCallback<ChangeEvent<string>>((evt) => { outputPath = evt.newValue; });
            root.Q<Button>("BuildAppOutputPathBrowse").clicked += () =>
            {
                output_path.value = EditorTool.Browse(true);
                outputPath = output_path.value;
            };
            root.Q<Button>("BuildAppApply").clicked += () => { ApplyConfig(); };
            root.Q<Button>("BuildApp").clicked += () =>
            {
                ApplyConfig();
                Build();
            };
        }

        public void OnUpdate()
        {
            
        }
        public void OnDestroy()
        {
            
        }

        private void Init()
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
                UIReferenceResolution = AppConfig.UIReferenceResolution;
            }
            else
            {
                ToolkitsWindow.ShowHelpBox("找不到AppConfig，请新建AppConfig");
            }
        }

        private void ApplyConfig()
        {
            AppConfig.IsDebug = DevelopmentBuild;
            AppConfig.LoadAssetsMold = LoadAssetsMold;
            AppConfig.IsTestServer = IsTestServer;
            AppConfig.AppFrameRate = AppFrameRate;
            AppConfig.TargetPackage = ApkTarget;
            AppConfig.NativeApp = NativeApp;
            AppConfig.ABPipeline = Pipeline;
            AppConfig.UIReferenceResolution = UIReferenceResolution;

            EditorUtility.SetDirty(AppConfig);

            HybridCLRSettings.Instance.enable = LoadAssetsMold != LoadAssetsMold.Native;

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

        private string assetsPath = "Assets/Resources/AssetsFolder";
        private string hybridPath = "Assets/Resources/HybridFolder";

        private void Build()
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

        private string[] GetBuildScenes()
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
        private string MoveAssetsToRoot(string path)
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
        private void MoveAssetsToOriginPath(string currentPath, string originPath)
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
        private string RelativePathToAbsolutePath(string relativePath)
        {
            return string.Format("{0}/{1}", Application.dataPath, relativePath.Replace("Assets/", ""));
        }

        /// <summary>
        /// 绝对路径转到相对路径
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        private string AbsolutePathToRelativePath(string absolutePath)
        {
            return absolutePath.Substring(absolutePath.IndexOf("Assets"));
        }

        #endregion
    }
}