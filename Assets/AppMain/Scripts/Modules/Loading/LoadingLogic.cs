/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 进度条场景(Logic) - 加载场景过渡功能
 * ===============================================
 * */

using System;
using App;
using AppFrame.Attribute;
using AppFrame.Interface;
using AppFrame.Tools;
using AppFrame.View;
using UnityEngine.SceneManagement;

namespace Modules.Loading
{
    [LogicOf(Assets.Global)]
    public class LoadingLogic : SingletonEvent<LoadingLogic>, ILogic
    {
        private LoadingView view;
        public LoadingLogic()
        {

        }
        public void Begin()
        {
            view = ViewManager.Instance.GetView<LoadingView>();
        }

        public void LoadScene(string sceneName, bool isLoading = false, Action callback = null, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            view.SetViewActive(isLoading);
            Root.LoadScene(sceneName, isLoading, progress =>
            {
                view.SetLoadingSliderValue(progress);
                if (progress >= 1)
                {
                    view.SetViewActive(false);
                    callback?.Invoke();
                }
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