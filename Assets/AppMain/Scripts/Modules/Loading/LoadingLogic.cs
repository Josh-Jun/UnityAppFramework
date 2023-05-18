using System;
using System.Collections;
using System.Collections.Generic;
using App;
using AppFrame.Data;
using AppFrame.Interface;
using AppFrame.Manager;
using AppFrame.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Loading
{
    public class LoadingLogic : SingletonEvent<LoadingLogic>, ILogic
    {
        private LoadingView view;
        public LoadingLogic()
        {

        }
        public void Begin()
        {
            if (view == null)
            {
                //加载窗体
                view = AssetsManager.Instance.LoadUIView<LoadingView>(AssetsPathConfig.LoadingView);
            }
        }

        public void LoadScene(string sceneName, bool isLoading = false, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            view.SetViewActive(isLoading);
            Root.LoadScene(sceneName, isLoading, progress =>
            {
                view.SetLoadingSliderValue(progress);
                if (progress >= 1)
                {
                    view.SetViewActive(false);
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