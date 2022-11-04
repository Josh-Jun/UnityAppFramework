using System;
using System.Collections;
using System.Collections.Generic;
using AppFramework.Data;
using AppFramework.Interface;
using AppFramework.Manager;
using AppFramework.Tools;
using UnityEngine;

namespace App.Ask
{
    public class AskLogic : SingletonEvent<AskLogic>, ILogic
    {
        public AskView view;
        public AskLogic()
        {
            AddEventMsg<string, Action, Action>("ShowAskView", ShowAskView);
            AddEventMsg<string, float>("ShowTips", ShowTips);
        }
        public void Begin()
        {
            if (view == null)
            {
                view = AssetsManager.Instance.LoadUIView<AskView>(AssetsPathConfig.AskView);
            }
        }
        public void End()
        {
            view.SetViewActive(false);
        }
        public void ShowAskView(string content, Action confirm_callback = null, Action cancel_callback = null)
        {
            view.SetViewActive();
            view.SetViewInfo(content, confirm_callback, cancel_callback);
        }

        public void ShowTips(string content, float time = 2f)
        {
            view.SetViewActive();
            view.SetTips(content, time);
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