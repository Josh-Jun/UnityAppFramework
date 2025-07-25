using System;
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
        private ResourcePackage builtinPackage = null;
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
            builtinPackage = await Assets.CreatePackageAsync(AssetPackage.BuiltinPackage, true);
            // 请求资源清单的版本信息
            var request = builtinPackage.RequestPackageVersionAsync();
            await request.Task;
            if (request.Status == EOperationStatus.Succeed)
            {
                var update = builtinPackage.UpdatePackageManifestAsync(request.PackageVersion);
                await update.Task;
                if (update.Status == EOperationStatus.Succeed)
                {
                    var downloader = builtinPackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
                    //没有需要下载的资源
                    if (downloader.TotalDownloadCount == 0)
                    {        
                        Debug.Log("没有需要下载的资源");
                        EnterApp();
                        return;
                    }
                    downloader.OnDownloadOverCallback = OnDownloadOverCallback;
                    downloader.OnDownloadErrorCallback += OnDownloadErrorCallback;
                    downloader.OnDownloadProgressCallback += OnDownloadProgressCallback;
                    downloader.OnStartDownloadFileCallback += OnStartDownloadFileCallback;
                    downloader.BeginDownload();
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
            Debug.Log("加载热更dll");
            if (Global.AppConfig.AssetPlayMode != EPlayMode.EditorSimulateMode)
            {
                LoadMetadataForAOTAssemblies();
                LoadHotfixAssemblies();
            }
            Debug.Log("加载AppScene场景");
            // 加载AppScene
            Assets.LoadSceneAsync(AssetPath.AppScene);
        }

        private void OnDownloadOverCallback(bool isSucceed)
        {
            if (!isSucceed)
            {
                Debug.LogError($"{AssetPackage.BuiltinPackage}下载失败");
                return;
            }
            EnterApp();
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
                var ta = builtinPackage.LoadAssetSync($"{Global.DllBasePath}/{aotName}.bytes").AssetObject as TextAsset;
                if (ta != null) RuntimeApi.LoadMetadataForAOTAssembly(ta.bytes, HomologousImageMode.SuperSet);
            }
        }

        private void LoadHotfixAssemblies()
        {
            foreach (var assemblyName in Global.HotfixAssemblyNames)
            {
                var ta = builtinPackage.LoadAssetSync($"{Global.DllBasePath}/{assemblyName}.bytes").AssetObject as TextAsset;
                if (ta != null)
                {
                    var assembly = System.Reflection.Assembly.Load(ta.bytes);
                    Global.AssemblyPairs.Add(assemblyName, assembly);
                }
            }
        }
    }
}