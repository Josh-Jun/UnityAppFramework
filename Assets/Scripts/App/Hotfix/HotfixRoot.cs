using EventController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class HotfixRoot : SingletonEvent<HotfixRoot>, IRoot
    {
        private HotfixWindow hotfixWin;
        public HotfixRoot()
        {
            string prefab_HotfixPath = "App/Hotfix/Windows/HotfixWin";
            hotfixWin = (HotfixWindow)UIRoot.Instance.LoadLocalWindow(prefab_HotfixPath);
        }

        public void Begin()
        {
            //SendEventMsg("HotfixWin", true);
            //hotfixWin.SetWindowActive(true);
            hotfixWin.StartHotfix();
        }

        public void End()
        {

        }
    }
}