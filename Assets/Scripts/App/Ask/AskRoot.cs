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
                window = AssetsManager.Instance.LoadUIWindow<AskWindow>(AssetsPathConfig.AskWindow);
            }
        }
        public void End()
        {
            window.SetWindowActive(false);
        }
        public void ShowAskWindow(string content, Action confirm_callback = null, Action cancel_callback = null)
        {
            window.SetWindowActive();
            window.SetWindowInfo(content, confirm_callback, cancel_callback);
        }

        public void ShowTips(string content, float time = 2f)
        {
            window.SetWindowActive();
            window.SetTips(content, time);
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