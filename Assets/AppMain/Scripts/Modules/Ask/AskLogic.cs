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
            AddEventMsg<AskData>("ShowAskView", ShowAskView);
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
        public void ShowAskView(AskData askData)
        {
            view.SetViewActive();
            view.SetViewInfo(askData);
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