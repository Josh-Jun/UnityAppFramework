using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ask
{
    public class AskRoot : SingletonEvent<AskRoot>, IRoot
    {
        public AskWindow askWindow;
        public AskRoot()
        {
            AddEventMsg<string, Action, Action>("ShowAskWindow", ShowAskWindow);
            AddEventMsg<string, float>("ShowTips", ShowTips);
        }
        public void Begin()
        {
            if (askWindow == null)
            {
                string prefab_AskPath = "Ask/Assets/Windows/AskWindow";
                askWindow = AssetsManager.Instance.LoadUIWindow<AskWindow>(prefab_AskPath);
                askWindow.SetWindowActive();
            }
        }
        public void End()
        {
            Debug.Log("AskRoot End");
        }
        public void ShowAskWindow(string content, Action confirm_callback = null, Action cancel_callback = null)
        {
            askWindow.SetWindowInfo(content, confirm_callback, cancel_callback);
            askWindow.SetAsLastSibling();
        }

        public void ShowTips(string content, float time = 2f)
        {
            askWindow.SetTips(content, time);
            askWindow.SetAsLastSibling();
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