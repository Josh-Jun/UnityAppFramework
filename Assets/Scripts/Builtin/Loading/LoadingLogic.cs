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
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace App.Modules
{
    [LogicOf("Loading", AssetPath.LoadingScene)]
    public class LoadingLogic : EventBase, ILogic
    {
        private LoadingView View => ViewMaster.Instance.GetView<LoadingView>();

        public LoadingLogic()
        {
            AddEventMsg<object>("OpenLoadingView", OpenLoadingView);
            AddEventMsg("CloseLoadingView", CloseLoadingView);
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
        
        private int _timeId;

        private void LoadYAScene()
        {
            if (SceneMaster.Instance.CurrentScene != null)
            {
                SendEventMsg("BeforeLoadSceneEvent", SceneMaster.Instance.CurrentScene.Location);
            }

            var handle = Assets.LoadSceneAsync(SceneMaster.Instance.TargetScene.Location, AssetPackage.HotfixPackage);
            _timeId = TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                if (handle == null) return;
                SetLoadingSliderValue(handle.Progress, LoadingSceneEvent);
            });
            return;

            void LoadingSceneEvent()
            {
                SendEventMsg("AfterLoadSceneEvent", SceneMaster.Instance.TargetScene.Location);
                TimeUpdateMaster.Instance.EndTimer(_timeId);
            }
        }

        private async UniTask LoadABScene()
        {
            if (SceneMaster.Instance.CurrentScene != null)
            {
                SendEventMsg("BeforeLoadSceneEvent", SceneMaster.Instance.CurrentScene.Name);
            }

            var async = SceneManager.LoadSceneAsync(SceneMaster.Instance.TargetScene.Name);
            if (async != null)
            {
                async.allowSceneActivation = false;

                void LoadSceneCompleted()
                {
                    SendEventMsg("AfterLoadSceneEvent", SceneMaster.Instance.TargetScene.Name);
                }

                while (!async.isDone)
                {
                    await UniTask.DelayFrame(1);
                    var progressValue = async.progress < 0.9f ? async.progress : 1.0f;

                    // TODO 更新加载进度 
                    SetLoadingSliderValue(progressValue, LoadSceneCompleted);
                    if (progressValue >= 0.9f)
                    {
                        async.allowSceneActivation = true;
                    }
                }
            }
        }
        private void SetLoadingSliderValue(float value, Action callback = null)
        {
            View.LoadingSliderSlider.DOKill();
            var duration = value >= 1 ? 0.5f : 5f;
            var endValue = value >= 1 ? 1f : 0.9f;
            View.LoadingSliderSlider.DOValue(endValue, duration).SetEase(Ease.Linear).OnComplete(() => { callback?.Invoke(); });
            if(View.ProgressTextMeshProUGUI)
            {
                View.ProgressTextMeshProUGUI.text = $"{View.LoadingSliderSlider.value * 100:F2}%";
            }
        }
        
        #endregion

        #region View Logic

        private void OpenLoadingView(object obj)
        {
            switch (SceneMaster.Instance.TargetScene.Mold)
            {
                case LoadSceneMold.YAScene:
                    LoadYAScene();
                    break;
                case LoadSceneMold.ABScene:
                    LoadABScene().Forget();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CloseLoadingView()
        {
            
        }

        #endregion
    }
}