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
            //º”‘ÿ¥∞ÃÂ
            string prefab_LoadingPath = "Loading/Assets/Windows/LoadingWindow";
            loadingWin = this.LoadUIWindow<LoadingWindow>(prefab_LoadingPath, true);
        }
        public void End()
        {

        }
    }
}