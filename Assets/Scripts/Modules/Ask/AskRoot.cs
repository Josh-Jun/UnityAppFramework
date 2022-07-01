using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ask
{
    public class AskRoot : SingletonEvent<AskRoot>, IRoot
    {
        public AskWindow window;
        public AskRoot()
        {
            AddEventMsg<string, Action, Action>("ShowAskWindow", ShowAskWindow);
            AddEventMsg<string, float>("ShowTips", ShowTips);
        }
        public void Begin()
        {
            if (window == null)
            {
                string prefab_AskPath = "Ask/Assets/Windows/AskWindow";
                window = AssetsManager.Instance.LoadUIWindow<AskWindow>(prefab_AskPath);
                window.SetWindowActive();
            }
        }
        public void End()
        {
            window.SetWindowActive(false);
        }
        public void ShowAskWindow(string content, Action confirm_callback = null, Action cancel_callback = null)
        {
            window.SetWindowInfo(content, confirm_callback, cancel_callback);
            window.SetAsLastSibling();
        }

        public void ShowTips(string content, float time = 2f)
        {
            window.SetTips(content, time);
            window.SetAsLastSibling();
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