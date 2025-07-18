using System;
using System.Collections.Generic;
using System.IO;
using App.Editor.Helper;
using App.Runtime.Helper;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using YooAsset;
using YooAsset.Editor;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;
#if PICO_XR_SETTING
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
#endif

namespace App.Editor.View
{
    public class BuildApp : IToolkitEditor
    {
        private AppConfig AppConfig; //App配置表
        private bool EnableLog = false;
        private DevelopmentMold DevelopmentMold = DevelopmentMold.Sandbox;
        private EPlayMode AssetPlayMode = EPlayMode.EditorSimulateMode;
        private int AppFrameRate = 30;
        private ChannelPackage ChannelPackage = ChannelPackage.Default;
        private bool NativeApp = false;
        private string CDNVersion = string.Empty;
        private string outputPath;

        public void OnCreate(VisualElement root)
        {
            Init();
            var enable_log = root.Q<Toggle>("EnableLog");
            var development_mold = root.Q<EnumField>("DevelopmentMold");
            var asset_play_mode = root.Q<EnumField>("AssetPlayMode");
            var app_frame_rate = root.Q<TextField>("AppFrameRate");
            var channel_package = root.Q<EnumField>("ChannelPackage");
            var export_project = root.Q<Toggle>("ExportProject");
            var cdn_version = root.Q<TextField>("CDNVersion");
            var output_path = root.Q<TextField>("BuildAppOutputPath");

            enable_log.value = EnableLog;
            development_mold.Init(DevelopmentMold);
            asset_play_mode.Init(AssetPlayMode);
            app_frame_rate.value = AppFrameRate.ToString();
            channel_package.Init(ChannelPackage);
            export_project.value = NativeApp;
            cdn_version.value = CDNVersion;
            output_path.value = outputPath;
            
            channel_package.style.display = 
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS || 
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? DisplayStyle.Flex : DisplayStyle.None;
            
            export_project.style.display = 
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS || 
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? DisplayStyle.Flex : DisplayStyle.None;
            
            
            enable_log.RegisterCallback<ChangeEvent<bool>>((evt) => { EnableLog = evt.newValue; });
            
            development_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (DevelopmentMold)System.Enum.Parse(typeof(DevelopmentMold), evt.newValue);
                DevelopmentMold = mold;
            });
            
            asset_play_mode.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (EPlayMode)System.Enum.Parse(typeof(EPlayMode), evt.newValue);
                AssetPlayMode = mold;
            });
            
            app_frame_rate.RegisterCallback<ChangeEvent<string>>((evt) => { AppFrameRate = int.Parse(evt.newValue); });
            
            channel_package.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (ChannelPackage)System.Enum.Parse(typeof(ChannelPackage), evt.newValue);
                ChannelPackage = mold;
            });
            
            export_project.RegisterCallback<ChangeEvent<bool>>((evt) => { NativeApp = evt.newValue; });
            
            cdn_version.RegisterCallback<ChangeEvent<string>>((evt) => { CDNVersion = evt.newValue; });

            output_path.RegisterCallback<ChangeEvent<string>>((evt) => { outputPath = evt.newValue; });
            root.Q<Button>("BuildAppOutputPathBrowse").clicked += () =>
            {
                output_path.value = EditorHelper.Browse(true);
                outputPath = output_path.value;
            };
            root.Q<Button>("ApplyConfig").clicked += Apply;
            root.Q<Button>("GenerateAndCopyDll").clicked += GenerateAndCopyDll;
            root.Q<Button>("BuildAsset").clicked += BuildAssets;
            root.Q<Button>("BuildApp").clicked += Build;
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
            AppConfig = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/Resources/App/AppConfig.asset");
            if (AppConfig != null)
            {
                EnableLog = AppConfig.EnableLog;
                DevelopmentMold = AppConfig.DevelopmentMold;
                AssetPlayMode = AppConfig.AssetPlayMode;
                AppFrameRate = AppConfig.AppFrameRate;
                ChannelPackage = AppConfig.ChannelPackage;
                NativeApp = AppConfig.NativeApp;
                CDNVersion = AppConfig.CDNVersion;
            }
            else
            {
                Log.I("找不到AppConfig，请新建AppConfig");
            }
        }

        private void Apply()
        {
            AppConfig.EnableLog = EnableLog;
            AppConfig.DevelopmentMold = DevelopmentMold;
            AppConfig.AssetPlayMode = AssetPlayMode;
            AppConfig.AppFrameRate = AppFrameRate;
            AppConfig.ChannelPackage = ChannelPackage;
            AppConfig.NativeApp = NativeApp;
            AppConfig.CDNVersion = CDNVersion;

            EditorUtility.SetDirty(AppConfig);
        }

        private static void SetBuildSetting()
        {
            var appConfig = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/Resources/App/AppConfig.asset");
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // PlayerSettings.Android.keystorePass = "";
                // PlayerSettings.Android.keyaliasName = "";
                // PlayerSettings.Android.keyaliasPass = "";
                EditorUserBuildSettings.exportAsGoogleAndroidProject = appConfig.NativeApp;
                
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
                if (ChannelPackage == ChannelPackage.PicoXR)
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

        private static void Build()
        {
            var appConfig = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/Resources/App/AppConfig.asset");
            var package = PlayerSettings.applicationIdentifier.ToLower();
            var channel = appConfig.ChannelPackage.ToString().ToLower();
            var version = PlayerSettings.bundleVersion;
            var develop =  appConfig.DevelopmentMold.ToString().ToLower();
            var date = $"{DateTime.Now.Year}{DateTime.Now.Month:00}{DateTime.Now.Day:00}";
            var suffix = appConfig.NativeApp || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS ? "" : ".apk";
            var name = $"{package}_{channel}_v{version}_{develop}_{date}{suffix}";
            
            EditorUserBuildSettings.SwitchActiveBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup,
                EditorUserBuildSettings.activeBuildTarget);
            
            SetBuildSetting();

            var outputPath = Application.dataPath.Replace("Assets", "App");;
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

            var buildPath = $"{outputPath}/{name}";
            var buildOption = BuildOptions.None;
            if (appConfig.EnableLog)
            {
                buildOption |= BuildOptions.Development;
            }

            buildOption |= BuildOptions.CleanBuildCache;
            buildOption |= BuildOptions.BuildScriptsOnly;
            buildOption |= BuildOptions.CompressWithLz4;
            // 只构建Launcher场景
            var scenes = new EditorBuildSettingsScene[1];
            scenes[0] = EditorBuildSettings.scenes[0];
            var report = BuildPipeline.BuildPlayer(scenes, buildPath, EditorUserBuildSettings.activeBuildTarget, buildOption);

            if (report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.RevealInFinder(buildPath);
                Debug.Log($"GenericBuild Success Path = {buildPath}");
            }
            else
            {
                Debug.Log($"Build Failed {report.SummarizeErrors()}");
            }
        }

        #region YooAssetBuild

        private static void BuildAssets()
        {
            FileUtil.DeleteFileOrDirectory(Application.streamingAssetsPath + "/AssetBundles");
            var appConfig = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/Resources/App/AppConfig.asset");
            var version = GetDefaultPackageVersion();
            YooAssetBuild(EditorUserBuildSettings.activeBuildTarget, AssetPackage.BuiltinPackage, version, EBuildinFileCopyOption.ClearAndCopyAll);
            switch (appConfig.AssetPlayMode)
            {
                case EPlayMode.OfflinePlayMode:
                    YooAssetBuild(EditorUserBuildSettings.activeBuildTarget, AssetPackage.HotfixPackage, version, EBuildinFileCopyOption.ClearAndCopyAll);
                    break;
                case EPlayMode.HostPlayMode:
                {
                    var buildParameters = YooAssetBuild(EditorUserBuildSettings.activeBuildTarget, AssetPackage.HotfixPackage, version, EBuildinFileCopyOption.None);
                    OnlyCopyPackageManifestFile(buildParameters);
                    break;
                }
                case EPlayMode.EditorSimulateMode:
                case EPlayMode.WebPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static ScriptableBuildParameters YooAssetBuild(BuildTarget buildTarget, AssetPackage package, string version, EBuildinFileCopyOption copyOption)
        {
            Debug.Log($"开始构建 : {buildTarget}");

            var buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            var streamingAssetsRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
    
            // 构建参数
            var buildParameters = new ScriptableBuildParameters
            {
                BuildOutputRoot = buildOutputRoot,
                BuildinFileRoot = streamingAssetsRoot,
                BuildPipeline = $"{EBuildPipeline.ScriptableBuildPipeline}",
                BuildBundleType = 2, //必须指定资源包类型
                BuildTarget = buildTarget,
                PackageName = $"{package}",
                PackageVersion = version,
                VerifyBuildingResult = true,
                EnableSharePackRule = true, //启用共享资源构建模式，兼容1.5x版本
                FileNameStyle = EFileNameStyle.HashName,
                BuildinFileCopyOption = copyOption,
                BuildinFileCopyParams = string.Empty,
                EncryptionServices = new EncryptionNone(),
                CompressOption = ECompressOption.LZ4,
                ClearBuildCacheFiles = false, //不清理构建缓存，启用增量构建，可以提高打包速度！
                UseAssetDependencyDB = true //使用资源依赖关系数据库，可以提高打包速度！
            };

            // 执行构建
            var pipeline = new ScriptableBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);
            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
                
                var config = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/Resources/App/AppConfig.asset");
                config.CDNVersion = buildParameters.PackageVersion;
                EditorUtility.SetDirty(config);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
            }
            return buildParameters;
        }

        private static string GetDefaultPackageVersion()
        {
            var totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return $"{DateTime.Now.Year}-{DateTime.Now.Month:00}-{DateTime.Now.Day:00}-{totalMinutes}";
        }

        private static void OnlyCopyPackageManifestFile(ScriptableBuildParameters buildParameters)
        {
            var rootDirectory = buildParameters.GetBuildinRootDirectory();
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, true);
            }

            Directory.CreateDirectory(rootDirectory);

            var dir = new DirectoryInfo(buildParameters.GetPackageOutputDirectory());
            var files = dir.GetFiles(); // 获取所有文件
            
            foreach (var file in files)
            {
                if (file.Name.StartsWith(buildParameters.PackageName) && !file.Name.EndsWith(".json"))
                {
                    File.Copy(file.FullName, Path.Combine(rootDirectory, file.Name));
                }
            }

        }
        
        #endregion
        
        #region GenerateAndCopyDll

        private static void GenerateAndCopyDll()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            PrebuildCommand.GenerateAll();
            CopyDLLToSourceData(buildTarget);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }


        private static void CopyDLLToSourceData(BuildTarget target)
        {
            var targetDstDir = $"{Application.dataPath}/Bundles/Builtin/Dlls";
            CopyAOTAssembliesToSourceData(target, targetDstDir);
            CopyMyAssembliesToSourceData(target, targetDstDir);
        }
        private static void CopyAOTAssembliesToSourceData(BuildTarget target, string targetDstDir)
        {
            var aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            Directory.CreateDirectory(targetDstDir);

            var list = new List<string>();
            list.AddRange(Global.AOTMetaAssemblyNames);
            foreach (var dll in list)
            {
                var srcDllPath = $"{aotAssembliesSrcDir}/{dll}";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError(
                        $"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }

                var dllBytesPath = $"{targetDstDir}/{dll}.bytes";

                Debug.Log($"[CopyAOTAssembliesToSourceData] copy AOT dll {srcDllPath} -> {dllBytesPath}");
                File.Copy(srcDllPath, dllBytesPath, true);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void CopyMyAssembliesToSourceData(BuildTarget target, string targetDstDir)
        {
            var aotAssembliesSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            Directory.CreateDirectory(targetDstDir);

            var list = new List<string>();
            list.AddRange(Global.HotfixAssemblyNames);
            foreach (var dll in list)
            {
                var srcDllPath = $"{aotAssembliesSrcDir}/{dll}";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError(
                        $"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }

                var dllBytesPath = $"{targetDstDir}/{dll}.bytes";

                Debug.Log($"[CopyMyAssembliesToSourceData] copy AOT dll {srcDllPath} -> {dllBytesPath}");
                File.Copy(srcDllPath, dllBytesPath, true);
            }
        }

        #endregion

        // Jenkins打包掉用
        public static void OneKeyBuild()
        {
            // 解析命令行参数
            var args = Environment.GetCommandLineArgs();
            var appConfig = AssetDatabase.LoadAssetAtPath<AppConfig>("Assets/Resources/App/AppConfig.asset");
            // 设置相关参数
            foreach (var arg in args)
            {
                Debug.Log($"解析到的命令行参数:[{arg}]");
                if (arg.Contains("--version:"))
                {
                    var version = arg.Split(':')[^1];
                    var code = version.Replace(".", "");
                    PlayerSettings.bundleVersion = version;
                    PlayerSettings.Android.bundleVersionCode = int.Parse(code);
                    PlayerSettings.iOS.buildNumber = code;
                }
                else if (arg.Contains("--development:"))
                {
                    var development = arg.Split(':')[^1];
                    var mold = (DevelopmentMold)Enum.Parse(typeof(DevelopmentMold), development);
                    appConfig.DevelopmentMold = mold;
                }
                else if (arg.Contains("--assetplaymold:"))
                {
                    var assetplaymold = arg.Split(':')[^1];
                    var mold = (EPlayMode)Enum.Parse(typeof(EPlayMode), assetplaymold);
                    appConfig.AssetPlayMode = mold;
                }
                else if (arg.Contains("--channel:"))
                {
                    var channel = arg.Split(':')[^1];
                    var mold = (ChannelPackage)Enum.Parse(typeof(ChannelPackage), channel);
                    appConfig.ChannelPackage = mold;
                }
            }
            
            // 保存AppConfig
            EditorUtility.SetDirty(appConfig);
            AssetDatabase.Refresh();
            
            // 开始构建
            Debug.Log("=============================================Start Build HybridCLR=============================================");
            GenerateAndCopyDll();
            Debug.Log("=============================================Start Build AssetBundle=============================================");
            BuildAssets();
            Debug.Log("=============================================Start Build App=============================================");
            Build();
            
            Debug.Log("Build Complete!!!");
        }
    }
}