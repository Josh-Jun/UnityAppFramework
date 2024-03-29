using System;
using System.Collections;
using System.Collections.Generic;
using AppFrame.Data;
using AppFrame.Interface;
using AppFrame.Manager;
using AppFrame.Tools;
using UnityEngine;

namespace Modules.Ask
{
    public class AskLogic : SingletonEvent<AskLogic>, ILogic
    {
        public AskView view;
        public AskLogic()
        {
            AddEventMsg<string, Action, Action, string, string>("ShowAskView", ShowAskView);
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
        public void ShowAskView(string content, Action confirm_callback, Action cancel_callback, string confirm, string cancel)
        {
            view.SetViewActive();
            view.SetViewInfo(content, confirm_callback, cancel_callback, confirm, cancel);
        }
        public void ShowAskView(string content, Action confirm_callback, Action cancel_callback)
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