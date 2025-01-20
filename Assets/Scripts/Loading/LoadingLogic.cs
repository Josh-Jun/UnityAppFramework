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
using UnityEngine.SceneManagement;

namespace App.Modules.Loading
{
    [LogicOf(AssetPath.Global)]
    public class LoadingLogic : SingletonEvent<LoadingLogic>, ILogic
    {
        private LoadingView view;
        public LoadingLogic()
        {

        }
        public void Begin()
        {
            view = ViewMaster.Instance.GetView<LoadingView>();
        }

        public void LoadScene(string sceneName, bool isLoading = false, Action callback = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            view.SetViewActive(isLoading);
            Root.LoadScene(sceneName, progress =>
            {
                view.SetLoadingSliderValue(progress, () =>
                {
                    callback?.Invoke();
                    view.SetViewActive(false);
                });
            }, loadSceneMode);
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