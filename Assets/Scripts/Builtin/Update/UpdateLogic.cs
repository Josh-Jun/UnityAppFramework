/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年7月28 16:7
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using App.Core;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace App.Modules
{
    [LogicOf(AssetPath.AppScene)]
    public class UpdateLogic : SingletonEvent<UpdateLogic>, ILogic
    {
        private UpdateView _view;
        private ResourceDownloaderOperation _downloader;
        public UpdateLogic()
        {
            AddEventMsg<object>("OpenUpdateView", OpenUpdateView);
            AddEventMsg("CloseUpdateView", CloseUpdateView);
            AddEventMsg("UpdateNow", UpdateNow);
        }
        public void Begin()
        {
            _view = ViewMaster.Instance.AddView<UpdateView>(AssetPath.UpdateView, ViewMold.UI2D, 2, false);
            UniTask.Void(async () =>
            {
                _downloader = await Assets.UpdatePackage(AssetPackage.HotfixPackage);
                if (_downloader.TotalDownloadCount == 0)
                {
                    Root.StartApp();
                }
                else
                {
                    _view.OpenView();
                    // 需要下载,弹出更新界面
                    _view.SetContentText($"有{_downloader.TotalDownloadCount}个资源需要下载,总大小{_downloader.TotalDownloadBytes / 1048576f:F2}M");
                    _view.SetUpdateTipsActive(true);
                }
            });
        }
        
        private void UpdateNow()
        {
            _view.SetUpdateTipsActive(false);
            _view.SetProgressBarActive(true);
            UniTask.Void(async () =>
            {
                _downloader.OnDownloadProgressCallback = _view.SetDownloadProgress;
                _downloader.BeginDownload();
                await _downloader.Task;
                await Assets.ClearPackageUnusedCacheBundleFiles(AssetPackage.HotfixPackage);
                _view.CloseView();
                Root.StartApp();
            });
        }
        
        private void OpenUpdateView(object obj)
        {
            _view.Reset();
        }

        private void CloseUpdateView()
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