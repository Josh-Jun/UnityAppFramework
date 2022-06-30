using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Loading
{
    public class LoadingRoot : SingletonEvent<LoadingRoot>, IRoot
    {
        private LoadingWindow loadingWin;
        public LoadingRoot()
        {

        }
        public void Begin()
        {
            //加载窗体
            string prefab_LoadingPath = "Loading/Assets/Windows/LoadingWindow";
            loadingWin = AssetsManager.Instance.LoadUIWindow<LoadingWindow>(prefab_LoadingPath);
            loadingWin.SetWindowActive();
        }
        public void End()
        {
            Debug.Log("LoadingRoot End");
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