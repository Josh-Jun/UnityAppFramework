/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 更新功能(Logic) - 1,资源更新 2,应用更新
 * ===============================================
 * */

using System;
using App.Core;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using App.Runtime.Hotfix;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace App.Modules.Update
{
    [LogicOf(AssetPath.AppScene)]
    public class UpdateLogic : SingletonEvent<UpdateLogic>, ILogic
    {
        private UpdateView view;
        private ResourceDownloaderOperation downloader;
        private const int downloadingMaxNum = 10;
        private const int failedTryAgain = 3;

        public UpdateLogic()
        {
            
        }

        public void Begin()
        {
            BeginHotfix().Forget();
        }

        private async UniTask BeginHotfix()
        {
            var go = AssetsMaster.Instance.AddChildSync(AssetPath.UpdateView, ViewMaster.Instance.UI2DPanels[0]);
            view = go.GetComponent<UpdateView>();
            view.Init(UpdateNow);
            var hotfixPackage = await Assets.CreatePackageAsync(AssetPackage.HotfixPackage);
            // 请求资源清单的版本信息
            var request = hotfixPackage.RequestPackageVersionAsync();
            await request.Task;
            if (request.Status == EOperationStatus.Succeed)
            {
                var update = hotfixPackage.UpdatePackageManifestAsync(request.PackageVersion);
                await update.Task;
                if (update.Status == EOperationStatus.Succeed)
                {
                    downloader = hotfixPackage.CreateResourceDownloader(10, 3);
                    if (downloader.TotalDownloadCount > 0)
                    {
                        // 需要下载,弹出更新界面
                        view.SetContentText($"");
                        view.SetUpdateTipsActive(true);
                    }
                    else
                    {
                        Object.Destroy(view.gameObject);
                        // 不需要下载,直接进入App
                        Root.StartApp();
                    }
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

        private void UpdateNow()
        {
            view.SetUpdateTipsActive(false);
            view.SetProgressBarActive(true);
            view.SetTipsText("下载中...");
            downloader.OnDownloadOverCallback = OnDownloadOverCallback;
            downloader.OnDownloadErrorCallback += OnDownloadErrorCallback;
            downloader.OnDownloadProgressCallback += OnDownloadProgressCallback;
            downloader.OnStartDownloadFileCallback += OnStartDownloadFileCallback;
            downloader.BeginDownload();
        }

        private void OnDownloadOverCallback(bool isSucceed)
        {
            if (!isSucceed)
            {
                Debug.LogError($"{AssetPackage.HotfixPackage}下载失败");
                return;
            }
            Object.Destroy(view.gameObject);
            Root.StartApp();
        }

        private void OnDownloadErrorCallback(string fileName, string error)
        {
        }

        private long historyDownload = 0;
        private float time = -1;

        private void OnDownloadProgressCallback(int totalDownloadCount, int currentDownloadCount,
            long totalDownloadBytes, long currentDownloadBytes)
        {
            var progress = currentDownloadBytes / (float)totalDownloadBytes;
            float speed = 0;
            if (time < 0)
            {
                time = Time.time;
                historyDownload = currentDownloadBytes;
            }
            else
            {
                speed = (currentDownloadBytes - historyDownload) / 1024f / 1024f / (Time.time - time);
                historyDownload = currentDownloadBytes;
                time = Time.time;
            }

            view.SetSpeedText(speed);
            view.SetProgressText(currentDownloadBytes / 1024f / 1024f, totalDownloadBytes / 1024f / 1024f);
            view.SetProgressValue(progress);
        }

        private void OnStartDownloadFileCallback(string fileName, long sizeBytes)
        {
        }

        public void End()
        {
        }

        public void AppPause(bool pause)
        {
        }

        public void AppFocus(bool focus)
        {
        }

        public void AppQuit()
        {
        }
    }
}