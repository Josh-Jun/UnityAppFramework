/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月5 9:35
 * function    :
 * ===============================================
 * */

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

public static class Assets
{
    private const int downloadingMaxNum = 10;
    private const int failedTryAgain = 3;
    private static string Platform
    {
        get
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget switch
            {
                UnityEditor.BuildTarget.iOS or
                    UnityEditor.BuildTarget.WebGL or
                    UnityEditor.BuildTarget.Android or
                    UnityEditor.BuildTarget.VisionOS => $"{UnityEditor.EditorUserBuildSettings.activeBuildTarget}",
                UnityEditor.BuildTarget.StandaloneWindows or
                    UnityEditor.BuildTarget.StandaloneWindows64 => $"Windows",
                UnityEditor.BuildTarget.StandaloneOSX => $"MacOS",
                _ => string.Empty
            };
#else
            return Application.platform switch
            {
                RuntimePlatform.VisionOS or
                    RuntimePlatform.Android => $"{Application.platform}",
                RuntimePlatform.IPhonePlayer => $"iOS",
                RuntimePlatform.OSXPlayer => $"MacOS",
                RuntimePlatform.WindowsPlayer => $"Windows",
                RuntimePlatform.WebGLPlayer => $"WebGL",
                _ => string.Empty
            };
#endif
        }
    }

    public static async UniTask<ResourcePackage> CreatePackageAsync(AssetPackage assetPackage, bool isBuiltin = false)
    {
        var package = YooAssets.CreatePackage($"{assetPackage}");
        if (isBuiltin)
        {
            YooAssets.SetDefaultPackage(package);
        }

        switch (Global.AppConfig.AssetPlayMode)
        {
            case EPlayMode.EditorSimulateMode:
                var simulateBuildParam = new EditorSimulateBuildParam
                {
                    PackageName = $"{assetPackage}" //指定包裹名称
                };
                var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(simulateBuildParam);

                var editorFileSystemParams =
                    FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult);
                var initEditorParameters = new EditorSimulateModeParameters
                {
                    EditorFileSystemParameters = editorFileSystemParams
                };
                var initEditorOperation = package.InitializeAsync(initEditorParameters);

                await initEditorOperation.Task;

                if (initEditorOperation.Status == EOperationStatus.Succeed)
                    Debug.Log("资源包初始化成功！");
                else
                    Debug.LogError($"资源包初始化失败：{initEditorOperation}");
                break;
            case EPlayMode.OfflinePlayMode:
                var builtinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
                var initBuiltinParameters = new OfflinePlayModeParameters
                {
                    BuildinFileSystemParameters = builtinFileSystemParams
                };
                var initBuiltinOperation = package.InitializeAsync(initBuiltinParameters);

                await initBuiltinOperation.Task;

                if (initBuiltinOperation.Status == EOperationStatus.Succeed)
                    Debug.Log("资源包初始化成功！");
                else
                    Debug.LogError($"资源包初始化失败：{initBuiltinOperation.Error}");
                break;
            case EPlayMode.HostPlayMode:
                var remoteHostServices = new RemoteServices(GetCdnServerAddress(), GetCdnServerAddress());
                var cacheFileSystemParams =
                    FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteHostServices);
                var hostFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

                var initHostParameters = new HostPlayModeParameters
                {
                    BuildinFileSystemParameters = hostFileSystemParams,
                    CacheFileSystemParameters = cacheFileSystemParams
                };
                var initHostOperation = package.InitializeAsync(initHostParameters);

                await initHostOperation.Task;

                if (initHostOperation.Status == EOperationStatus.Succeed)
                    Debug.Log("资源包初始化成功！");
                else
                    Debug.LogError($"资源包初始化失败：{initHostOperation.Error}");
                break;
            case EPlayMode.WebPlayMode:
                var remoteWebServices = new RemoteServices(GetCdnServerAddress(), GetCdnServerAddress());
                var webServerFileSystemParams = FileSystemParameters.CreateDefaultWebServerFileSystemParameters();
                var webRemoteFileSystemParams =
                    FileSystemParameters.CreateDefaultWebRemoteFileSystemParameters(remoteWebServices); //支持跨域下载

                var initWebParameters = new WebPlayModeParameters
                {
                    WebServerFileSystemParameters = webServerFileSystemParams,
                    WebRemoteFileSystemParameters = webRemoteFileSystemParams
                };

                var initWebOperation = package.InitializeAsync(initWebParameters);

                await initWebOperation.Task;

                if (initWebOperation.Status == EOperationStatus.Succeed)
                    Debug.Log("资源包初始化成功！");
                else
                    Debug.LogError($"资源包初始化失败：{initWebOperation.Error}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return package;
    }

    private static string GetCdnServerAddress()
    {
        return $"{Global.CdnServer}/AssetBundles/{Platform}/v{Application.version}";
    }

    public static async UniTask<(bool, string)> RequestPackageVersionAsync(AssetPackage assetPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var operation = package.RequestPackageVersionAsync();
        await operation.Task;
        return (operation.Status == EOperationStatus.Succeed, operation.PackageVersion);
    }

    public static async UniTask<bool> UpdatePackageManifestAsync(AssetPackage assetPackage, string packageVersion)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var operation = package.UpdatePackageManifestAsync(packageVersion);
        await operation.Task;
        return operation.Status == EOperationStatus.Succeed;
    }

    public static ResourceDownloaderOperation CreatePackageDownloader(AssetPackage assetPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        return downloader;
    }

    public static async UniTask BeginDownloadPackage(ResourceDownloaderOperation downloader,
        DownloaderOperation.OnDownloadOver OnDownloadFinishFunction,
        DownloaderOperation.OnDownloadError OnDownloadErrorCallback,
        DownloaderOperation.OnDownloadProgress OnDownloadProgressCallback,
        DownloaderOperation.OnStartDownloadFile OnStartDownloadFileCallback)
    {
        //注册回调方法
        downloader.OnDownloadOverCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
        downloader.OnDownloadErrorCallback = OnDownloadErrorCallback; //当下载器发生错误
        downloader.OnDownloadProgressCallback = OnDownloadProgressCallback; //当下载进度发生变化
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileCallback; //当开始下载某个文件

        //开启下载
        downloader.BeginDownload();
        await downloader.Task;
    }

    public static async UniTask DownloadPackageAsync(AssetPackage assetPackage,
        Action<int, long> callback,
        DownloaderOperation.OnDownloadOver OnDownloadFinishFunction,
        DownloaderOperation.OnDownloadError OnDownloadErrorCallback,
        DownloaderOperation.OnDownloadProgress OnDownloadProgressCallback,
        DownloaderOperation.OnStartDownloadFile OnStartDownloadFileCallback)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
        callback?.Invoke(downloader.TotalDownloadCount, downloader.TotalDownloadBytes);
        //注册回调方法
        downloader.OnDownloadOverCallback = OnDownloadFinishFunction; //当下载器结束（无论成功或失败）
        downloader.OnDownloadErrorCallback = OnDownloadErrorCallback; //当下载器发生错误
        downloader.OnDownloadProgressCallback = OnDownloadProgressCallback; //当下载进度发生变化
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileCallback; //当开始下载某个文件

        //开启下载
        downloader.BeginDownload();
        await downloader.Task;
    }
    
    public static async UniTask ClearPackageAllCacheBundleFiles(AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var operation = package.ClearCacheBundleFilesAsync(EFileClearMode.ClearAllBundleFiles);
        await operation.Task;

        if (operation.Status == EOperationStatus.Succeed)
        {
            //清理成功
            Debug.Log($"package {assetPackage} is clear");
        }
        else
        {
            //清理失败
            Debug.LogError(operation.Error);
        }
    }
    public static async UniTask ClearPackageUnusedCacheBundleFiles(AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var operation = package.ClearCacheBundleFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
        await operation.Task;

        if (operation.Status == EOperationStatus.Succeed)
        {
            //清理成功
            Debug.Log($"package {assetPackage} is clear");
        }
        else
        {
            //清理失败
            Debug.LogError(operation.Error);
        }
    }
    public static async UniTask ClearPackageCacheBundleFilesByTags(string[] tags, AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var operation = package.ClearCacheBundleFilesAsync(EFileClearMode.ClearBundleFilesByTags, tags);
        await operation.Task;

        if (operation.Status == EOperationStatus.Succeed)
        {
            //清理成功
            Debug.Log($"package {assetPackage} is clear");
        }
        else
        {
            //清理失败
            Debug.LogError(operation.Error);
        }
    }

    public static SceneHandle LoadSceneAsync(string location,
        AssetPackage assetPackage = AssetPackage.BuiltinPackage,
        LoadSceneMode loadSceneMode = LoadSceneMode.Single,
        bool suspendLoad = false)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var handle = package.LoadSceneAsync(location, loadSceneMode, default, suspendLoad);
        UniTask.Void(async () => await handle.Task);
        return handle;
    }

    public static AssetHandle LoadAssetSync(string location, AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var handle = package.LoadAssetSync(location);
        return handle;
    }
    public static async UniTask<AssetHandle> LoadAssetAsync(string location, AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var handle = package.LoadAssetSync(location);
        await handle.Task;
        return handle;
    }
    public static T LoadAssetSync<T>(string location, AssetPackage assetPackage = AssetPackage.BuiltinPackage) where T : UnityEngine.Object
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        return package.LoadAssetSync<T>(location).AssetObject as T;
    }
    public static async UniTask<T> LoadAssetAsync<T>(string location, AssetPackage assetPackage = AssetPackage.BuiltinPackage) where T : UnityEngine.Object
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var handle = package.LoadAssetAsync<T>(location);
        await handle.Task;
        return handle.AssetObject as T;
    }
    
    // 卸载所有引用计数为零的资源包。
    // 可以在切换场景之后调用资源释放方法或者写定时器间隔时间去释放。
    public static async UniTask UnloadUnusedAssets(AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var operation = package.UnloadUnusedAssetsAsync();
        await operation.Task;
    }

    // 强制卸载所有资源包，该方法请在合适的时机调用。
    // 注意：Package在销毁的时候也会自动调用该方法。
    public static async UniTask ForceUnloadAllAssets(AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        var operation = package.UnloadAllAssetsAsync();
        await operation.Task;
    }

    // 尝试卸载指定的资源对象
    // 注意：如果该资源还在被使用，该方法会无效。
    public static void TryUnloadUnusedAsset(string location, AssetPackage assetPackage = AssetPackage.BuiltinPackage)
    {
        var package = YooAssets.GetPackage($"{assetPackage}");
        package.TryUnloadUnusedAsset(location);
    }
}


public class RemoteServices: IRemoteServices
{
    private readonly string _defaultHostServer;
    private readonly string _fallbackHostServer;

    public RemoteServices(string defaultHostServer, string fallbackHostServer)
    {
        _defaultHostServer = defaultHostServer;
        _fallbackHostServer = fallbackHostServer;
    }

    string IRemoteServices.GetRemoteMainURL(string fileName)
    {
        return $"{_defaultHostServer}/{fileName}";
    }

    string IRemoteServices.GetRemoteFallbackURL(string fileName)
    {
        return $"{_fallbackHostServer}/{fileName}";
    }
}