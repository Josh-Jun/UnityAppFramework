using EventController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hotfix
{
    public class HotfixRoot : SingletonEvent<HotfixRoot>, IRoot
    {
        private HotfixWin hotfixWin;
        public HotfixRoot()
        {
            string prefab_HotfixPath = "App/Hotfix/Windows/HotfixWin";
            hotfixWin = UIRoot.Instance.LoadLocalWindow<HotfixWin>(prefab_HotfixPath);
        }

        public void Begin()
        {
            //SendEventMsg("HotfixWin", true);
            hotfixWin.SetWindowActive(true);
            hotfixWin.StartHotfix();
        }

        public void Finish()
        {

        }
    }
}