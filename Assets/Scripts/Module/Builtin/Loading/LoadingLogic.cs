/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 进度条场景(Logic) - 加载场景过渡功能
 * ===============================================
 * */

using System;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace App.Modules
{
    [LogicOf("Loading", AssetPath.LoadingScene)]
    public class LoadingLogic : EventBase, ILogic
    {
        private LoadingView View => ViewMaster.Instance.GetView<LoadingView>();

        public LoadingLogic()
        {
            AddEventMsg<ViewBaseData>("OpenLoadingView", OpenLoadingView);
            AddEventMsg("CloseLoadingView", CloseLoadingView);
            AddEventMsg<float>("LoadingSliderSliderEvent", arg => { });
        }

        #region Life Cycle
        
        public void Begin()
        {
            View.OpenView();
        }

        public void End()
        {
            View.CloseView();
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
        
        #endregion

        #region Logic

        private void LoadScene()
        {
            targetProgress = 0;
            loading = true;
            
            switch (SceneMaster.Instance.TargetScene.Mold)
            {
                case LoadSceneMold.YAScene:
                    sceneHandle = Assets.LoadSceneAsync(SceneMaster.Instance.TargetScene.Location, AssetPackage.HotfixPackage);
                    break;
                case LoadSceneMold.ABScene:
                    asyncOperation = SceneManager.LoadSceneAsync(SceneMaster.Instance.TargetScene.Name);
                    if (asyncOperation != null) asyncOperation.allowSceneActivation = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private SceneHandle sceneHandle;
        private AsyncOperation asyncOperation;
        private int targetProgress;
        private bool loading;
        private void Update(float time)
        {
            if(!loading) return;
            if (asyncOperation != null)
            {
                if (asyncOperation.progress < 0.9f)
                {
                    targetProgress = (int) (asyncOperation.progress * 100);
                }
                else
                {
                    ++targetProgress;
                    if (targetProgress == 100)
                    {
                        asyncOperation.allowSceneActivation = true;
                    }
                }
            }

            if (sceneHandle != null)
            {
                targetProgress = (int) (sceneHandle.Progress * 100);
            }
            View.LoadingSliderSlider.value = targetProgress / 100f;
            if(View.ProgressTextMeshProUGUI)
                View.ProgressTextMeshProUGUI.text = $"{targetProgress}%";
            
            if (targetProgress < 100) return;
            TimeUpdateMaster.Instance.EndTimer(loadingTimeTaskId);
            SendEventMsg("AfterLoadSceneEvent", SceneMaster.Instance.TargetScene.Name);
            asyncOperation = null;
            sceneHandle = null;
            loading = false;
        }
        
        #endregion

        #region View Logic

        private int loadingTimeTaskId = -1;
        private void OpenLoadingView(ViewBaseData baseData)
        {
            loadingTimeTaskId = TimeUpdateMaster.Instance.StartTimer(Update);
            
            if (SceneMaster.Instance.CurrentScene != null)
            {
                SendEventMsg("BeforeLoadSceneEvent", SceneMaster.Instance.CurrentScene.Name);
            }

            LoadScene();
        }

        private void CloseLoadingView()
        {
            
        }

        #endregion
    }
}