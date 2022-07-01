using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loading
{
    public class LoadingRoot : SingletonEvent<LoadingRoot>, IRoot
    {
        private LoadingWindow window;
        public LoadingRoot()
        {

        }
        public void Begin()
        {
            if (window == null)
            {
                //加载窗体
                string prefab_LoadingPath = "Loading/Assets/Windows/LoadingWindow";
                window = AssetsManager.Instance.LoadUIWindow<LoadingWindow>(prefab_LoadingPath);
            }
        }

        public void Load(string sceneName, Action callback)
        {
            window.SetWindowActive();
            window.StartCoroutine(window.StartLoadScene(sceneName, callback));
        }
        public void End()
        {
            window.SetWindowActive(false);
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