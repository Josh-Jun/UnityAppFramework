using System;
using System.Linq;
using App.Runtime.Helper;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace App.Runtime
{
    public class Launcher : MonoBehaviour
    {
        private bool isNeedRestart;
        private ResourcePackage builtinPackage = null;
        private Hotfix hotfix = null;
        
        private async void Awake()
        {
            try
            {
                hotfix = transform.Find("Canvas").GetComponent<Hotfix>();
                Global.AppConfig = Resources.Load<AppConfig>("App/AppConfig");
                // YooAssets初始化
                YooAssets.Initialize();
                // 创建默认包
                builtinPackage = await Assets.CreatePackageAsync(AssetPackage.BuiltinPackage, true);
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
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnDownloaderResult(int totalDownloadCount, long totalDownloadBytes)
        {
            isNeedRestart = totalDownloadCount > 0; 
        }

        private void OnDownloadFinishFunction(bool isSucceed)
        {
            if (isNeedRestart)
            {
                hotfix.SetUpdateCompletePanelActive(true);
                return;
            }
            if (!isSucceed) return;
            if (Global.AppConfig.AssetPlayMode != EPlayMode.EditorSimulateMode)
            {
                LoadMetadataForAOTAssemblies();
                LoadHotfixAssemblies();
            }

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
                var ta = builtinPackage.LoadAssetAsync($"{Global.DllBasePath}/{aotName}").AssetObject as TextAsset;
                if (ta != null) RuntimeApi.LoadMetadataForAOTAssembly(ta.bytes, HomologousImageMode.SuperSet);
            }
        }

        private void LoadHotfixAssemblies()
        {
            foreach (var assemblyName in Global.HotfixAssemblyNames)
            {
                var ta = builtinPackage.LoadAssetAsync($"{Global.DllBasePath}/{assemblyName}").AssetObject as TextAsset;
                if (ta != null) System.Reflection.Assembly.Load(ta.bytes);
            }
        }
    }
}