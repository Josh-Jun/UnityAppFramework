using System;
using App.Runtime.Helper;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace App.Runtime
{
    public class Launcher : MonoBehaviour
    {
        private ResourcePackage builtinPackage = null;
        private Hotfix hotfix = null;

        private void Awake()
        {
            Init().Forget();
        }
        private async UniTask Init()
        {
            hotfix = transform.Find("Canvas").GetComponent<Hotfix>();
            Global.AppConfig = Resources.Load<AppConfig>("App/AppConfig");
            // YooAssets初始化
            YooAssets.Initialize();
            // 创建默认包
            builtinPackage = await Assets.CreatePackageAsync(AssetPackage.BuiltinPackage, true);
            YooAssets.SetDefaultPackage(builtinPackage);
            // 请求资源清单的版本信息
            var (request_result, version) = await Assets.RequestPackageVersionAsync(AssetPackage.BuiltinPackage);
            if (request_result)
            {
                var update_result = await Assets.UpdatePackageManifestAsync(AssetPackage.BuiltinPackage, version);
                if (update_result)
                {
                    await Assets.DownloadPackageAsync(AssetPackage.BuiltinPackage,
                        OnDownloaderResult,
                        OnDownloadFinishFunction,
                        OnDownloadErrorCallback,
                        OnDownloadProgressCallback,
                        OnStartDownloadFileCallback);
                }
                else
                {
                    Debug.LogError($"Failed to update package manifest: {version}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load package manifest: {AssetPackage.BuiltinPackage}");
            }
        }

        private void OnDownloaderResult(int totalDownloadCount, long totalDownloadBytes)
        {
            
        }

        private void OnDownloadFinishFunction(bool isSucceed)
        {
            if (!isSucceed) return;
            if (Global.AppConfig.AssetPlayMode != EPlayMode.EditorSimulateMode)
            {
                LoadMetadataForAOTAssemblies();
                LoadHotfixAssemblies();
            }
            Debug.Log("加载AppScene场景");
            // 加载AppScene
            Assets.LoadSceneAsync(AssetPath.AppScene);
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
                if (ta != null) System.Reflection.Assembly.Load(ta.bytes);
            }
        }
    }
}