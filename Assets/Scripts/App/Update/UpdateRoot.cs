using Data;
using EventController;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Update
{
    public class UpdateRoot : SingletonEvent<UpdateRoot>, IRoot
    {
        private UpdateWindow updateWin;
        public UpdateRoot()
        {
            AddEventMsg("UpdateNow", () => { UpdateNow(); });
        }
        public void Begin()
        {
            string prefab_UpdatePath = "App/Update/Windows/UpdateWindow";
            updateWin = this.LoadLocalWindow<UpdateWindow>(prefab_UpdatePath);
            updateWin.SetWindowActive();

            updateWin.SetTipsText("检查更新中...");
            updateWin.SetProgressValue(0);
            UpdateManager.Instance.StartUpdate((bool isUpdate, string des) =>
            {
                if (isUpdate)
                {
                    updateWin.SetContentText(des);
                    updateWin.SetUpdateTipsActive(true);
                }
                else
                {
                    LoadAssetBundle();
                }
            });
        }
        private void UpdateNow()
        {
            updateWin.SetUpdateTipsActive(false);
            updateWin.SetProgressBarActive(true);
            updateWin.SetTipsText("下载中...");
            UpdateManager.Instance.DownLoadAssetBundle();
            App.app.StartCoroutine(DownLoading());

        }
        private IEnumerator DownLoading()
        {
            float time = 0;
            float previousSize = 0;
            float speed = 0;
            while (UpdateManager.Instance.GetProgress() != 1)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
                if (time >= 1f)
                {
                    speed = (UpdateManager.Instance.GetLoadedSize() - previousSize);
                    previousSize = UpdateManager.Instance.GetLoadedSize();
                    time = 0;
                }
                updateWin.SetSpeedText(speed);
                updateWin.SetProgressText(UpdateManager.Instance.GetLoadedSize(), UpdateManager.Instance.LoadTotalSize);
                updateWin.SetProgressValue(UpdateManager.Instance.GetProgress());
            }
            //更新下载完成，加载AB包
            LoadAssetBundle();
        }
        private void LoadAssetBundle()
        {
            updateWin.SetTipsText("正在加载资源...");
            updateWin.SetProgressBarActive(true);
            UpdateManager.Instance.LoadAssetBundle((bool isEnd, string bundleName, float bundleProgress) =>
            {
                updateWin.SetTipsText(string.Format("正在加载资源:{0}资源包...", bundleName));
                if (isEnd && bundleProgress == 1)
                {
                    updateWin.SetProgressBarActive(false);
                    //AB包加载完成
                    Root.InitRootScripts(() => { Root.LoadScene(Root.MainSceneName); });
                }
            });
        }

        public void End()
        {

        }
    }
}