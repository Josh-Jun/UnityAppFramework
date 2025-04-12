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
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace App.Modules.Update
{
    [LogicOf(AssetPath.AppScene)]
    public class UpdateLogic : SingletonEvent<UpdateLogic>, ILogic
    {
        private UpdateView view;
        private ResourceDownloaderOperation downloader;

        public UpdateLogic()
        {
            AddEventMsg("UpdateNow", UpdateNow);
        }

        public void Begin()
        {
            BeginHotfix().Forget();
        }

        private async UniTask BeginHotfix()
        {
            var view_prefab = AssetsMaster.Instance.LoadAssetSync<GameObject>(AssetPath.UpdateView);
            view = ViewMaster.Instance.AddView<UpdateView>(view_prefab);
            await Assets.CreatePackageAsync(AssetPackage.HotfixPackage);
            // 请求资源清单的版本信息
            var (request_result, version) = await Assets.RequestPackageVersionAsync(AssetPackage.HotfixPackage);
            if (request_result)
            {
                var update_result = await Assets.UpdatePackageManifestAsync(AssetPackage.HotfixPackage, version);
                if (update_result)
                {
                    downloader = Assets.CreatePackageDownloader(AssetPackage.HotfixPackage);
                    if (downloader.TotalDownloadCount > 0)
                    {
                        // 需要下载,弹出更新界面
                        view.SetContentText($"");
                        view.SetUpdateTipsActive(true);
                    }
                    else
                    {
                        view.SetViewActive(false);
                        // 不需要下载,直接进入App
                        Root.StartApp();
                    }
                }
                else
                {
                    Debug.LogError($"Failed to update package manifest: {version}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load package manifest: {AssetPackage.HotfixPackage}");
            }
        }

        private void UpdateNow()
        {
            view.SetUpdateTipsActive(false);
            view.SetProgressBarActive(true);
            view.SetTipsText("下载中...");
            Assets.BeginDownloadPackage(downloader,
                OnDownloadFinishFunction,
                OnDownloadErrorCallback,
                OnDownloadProgressCallback,
                OnStartDownloadFileCallback);
        }

        private void OnDownloadFinishFunction(bool isSucceed)
        {
            if (!isSucceed) return;
            view.SetViewActive(false);
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
                speed = (currentDownloadBytes - historyDownload) / (Time.time - time);
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