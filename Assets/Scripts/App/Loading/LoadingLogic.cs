using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loading
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

        public void Loading(float progress, Action callback)
        {
            if (!view.GetViewActive())
            {
                view.SetViewActive(true);
            }
            view.SetLoadingSliderValue(progress);
            if (progress >= 1)
            {
                view.SetViewActive(false);
                callback?.Invoke();
            }
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