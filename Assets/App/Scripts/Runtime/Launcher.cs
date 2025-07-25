using System;
using System.Linq;
using App.Runtime.Helper;
using App.Runtime.Hotfix;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace App.Runtime
{
    public class Launcher : MonoBehaviour
    {
        private HotfixView hotfix = null;
        private const int downloadingMaxNum = 10;
        private const int failedTryAgain = 3;

        private void Awake()
        {
            Init().Forget();
        }
        private async UniTask Init()
        {
            hotfix = transform.Find("Canvas").GetComponent<HotfixView>();
            Global.AppConfig = Resources.Load<AppConfig>("App/AppConfig");
            // YooAssets初始化
            YooAssets.Initialize();
            // 创建默认包
            await UpdatePackage(AssetPackage.BuiltinPackage);
            Debug.Log("加载热更dll");
            if (Global.AppConfig.AssetPlayMode != EPlayMode.EditorSimulateMode)
            {
                LoadMetadataForAOTAssemblies();
                LoadHotfixAssemblies();
            }
            await UpdatePackage(AssetPackage.HotfixPackage);
            // 加载AppScene
            await Assets.LoadSceneAsync(AssetPath.AppScene);
        }

        private async UniTask UpdatePackage(AssetPackage assetPackage, bool isBuiltinPackage = false)
        {
            var package = await Assets.CreatePackageAsync(assetPackage, isBuiltinPackage);
            // 请求资源清单的版本信息
            var request = package.RequestPackageVersionAsync();
            await request.Task;
            if (request.Status == EOperationStatus.Succeed)
            {
                var update = package.UpdatePackageManifestAsync(request.PackageVersion);
                await update.Task;
                if (update.Status == EOperationStatus.Succeed)
                {
                    var downloader = package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
                    //没有需要下载的资源
                    if (downloader.TotalDownloadCount == 0)
                    {
                        Debug.Log("没有需要下载的资源");
                        return;
                    }
                    downloader.OnDownloadOverCallback = OnDownloadOverCallback;
                    downloader.OnDownloadErrorCallback += OnDownloadErrorCallback;
                    downloader.OnDownloadProgressCallback += OnDownloadProgressCallback;
                    downloader.OnStartDownloadFileCallback += OnStartDownloadFileCallback;
                    downloader.BeginDownload();
                    await downloader.Task;
                }
                else
                {
                    Debug.LogError($"Failed to update package manifest: {update.Error}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load package manifest: {request.Error}");
            }
        }

        private void EnterApp()
        {
            Debug.Log("加载AppScene场景");
        }

        private void OnDownloadOverCallback(bool isSucceed)
        {
            if (!isSucceed)
            {
                Debug.LogError($"{AssetPackage.BuiltinPackage}下载失败");
                return;
            }
        }

        private void OnDownloadErrorCallback(string fileName, string error)
        {
            Debug.LogError($"Failed to load file: {fileName}, error: {error}");
        }

        private void OnDownloadProgressCallback(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            hotfix.SetDownloadProgress(totalDownloadCount, currentDownloadCount, totalDownloadBytes, currentDownloadBytes);
        }

        private void OnStartDownloadFileCallback(string fileName, long sizeBytes)
        {
        }

        private void LoadMetadataForAOTAssemblies()
        {
            foreach (var aotName in Global.AOTMetaAssemblyNames)
            {
#if !UNITY_EDITOR
                var ta = Assets.LoadAssetSync($"{Global.DllBasePath}/{aotName}.bytes").AssetObject as TextAsset;
                if (ta != null) RuntimeApi.LoadMetadataForAOTAssembly(ta.bytes, HomologousImageMode.SuperSet);
#endif
            }
        }

        private void LoadHotfixAssemblies()
        {
            foreach (var assemblyName in Global.HotfixAssemblyNames)
            {
#if !UNITY_EDITOR
                var ta = Assets.LoadAssetSync($"{Global.DllBasePath}/{assemblyName}.dll.bytes").AssetObject as TextAsset;
                if (ta != null)
                {
                    var assembly = System.Reflection.Assembly.Load(ta.bytes);
                    Global.AssemblyPairs.Add(assemblyName, assembly);
                }
#else
                var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == assemblyName);
                Global.AssemblyPairs.Add(assemblyName, assembly);
#endif
            }
        }
    }
}