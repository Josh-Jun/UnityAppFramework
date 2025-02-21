/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 进度条场景(Logic) - 加载场景过渡功能
 * ===============================================
 * */

using System;
using App.Core;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using Modules.SceneLoader;
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
            view.SetViewActive(true);
            var targetScene = SceneLoaderLogic.Instance.CurrentScene;
            var handle = Assets.LoadSceneAsync(targetScene, AssetPackage.HotfixPackage);
            var time_id = TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                if (handle == null) return;
                view.SetLoadingSliderValue(handle.Progress);
            });
            handle.Completed += sceneHandle =>
            {
                TimeUpdateMaster.Instance.EndTimer(time_id);
                view.SetLoadingSliderValue(handle.Progress);
            };
        }
        public void End()
        {
            view.SetViewActive(false);
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