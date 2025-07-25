/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 进度条场景(Logic) - 加载场景过渡功能
 * ===============================================
 * */

using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace App.Modules.Loading
{
    [LogicOf(AssetPath.LoadingScene)]
    public class LoadingLogic : SingletonEvent<LoadingLogic>, ILogic
    {
        private LoadingView view;
        public LoadingLogic()
        {

        }
        public void Begin()
        {
            view = ViewMaster.Instance.GetView<LoadingView>();
            LoadScene();
        }

        private void LoadScene()
        {
            view.OpenView();
            switch (SceneMaster.Instance.TargetScene.Mold)
            {
                case LoadSceneMold.YAScene:
                    LoadYAScene();
                    break;
                case LoadSceneMold.ABScene:
                    LoadABScene().Forget();
                    break;
            }

        }
        private int timeId;
        private void LoadYAScene()
        {
            if (SceneMaster.Instance.CurrentScene != null)
            {
                SendEventMsg("BeforeLoadSceneEvent", SceneMaster.Instance.CurrentScene.Location);
            }
            var handle = Assets.LoadSceneAsync(SceneMaster.Instance.TargetScene.Location, AssetPackage.HotfixPackage);
            void LoadingSceneEvent()
            {
                SendEventMsg("AfterLoadSceneEvent", SceneMaster.Instance.TargetScene.Location);
                TimeUpdateMaster.Instance.EndTimer(timeId);
            }
            timeId = TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                if (handle == null) return;
                view.SetLoadingSliderValue(handle.Progress, LoadingSceneEvent);
            });
        }

        private async UniTask LoadABScene()
        {
            if (SceneMaster.Instance.CurrentScene != null)
            {
                SendEventMsg("BeforeLoadSceneEvent", SceneMaster.Instance.CurrentScene.Name);
            }
            var async = SceneManager.LoadSceneAsync(SceneMaster.Instance.TargetScene.Name);
            async.allowSceneActivation = false;
            void LoadSceneCompleted()
            {
                SendEventMsg("AfterLoadSceneEvent", SceneMaster.Instance.TargetScene.Name);
            }
            float progressValue = 0;
            while (!async.isDone)
            {
                await UniTask.DelayFrame(1);
                if (async.progress < 0.9f)
                {
                    progressValue = async.progress;
                }
                else
                {
                    progressValue = 1.0f;

                }
                // TODO 更新加载进度 
                view.SetLoadingSliderValue(progressValue, LoadSceneCompleted);
                if (progressValue >= 0.9f)
                {
                    async.allowSceneActivation = true;
                }
            }
        }

        public void End()
        {
            view.CloseView();
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