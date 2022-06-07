using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ask
{
    public class AskRoot : SingletonEvent<AskRoot>, IRoot
    {
        public AskWindow askWindow;
        public Action confirmCallback;
        public Action cancelCallback;
        public AskRoot()
        {
            AddEventMsgParams("ShowAskWindow", (object[] args) => { ShowAskWindow((string)args[0], (Action)args[1], (Action)args[2]); });
            AddEventMsg("OnConfirmEvent", OnConfirmEvent);
            AddEventMsg("OnCancelEvent", OnCancelEvent);
        }
        public void Begin()
        {
            if (askWindow == null)
            {
                string prefab_AskPath = "Ask/Assets/Windows/AskWindow";
                askWindow = this.LoadUIWindow<AskWindow>(prefab_AskPath);
            }
        }
        public void ShowAskWindow(string content, Action confirm_callback = null, Action cancel_callback = null)
        {
            askWindow.SetWindowInfo(content);
            confirmCallback = confirm_callback;
            cancelCallback = cancel_callback;
            askWindow.SetWindowActive();
            askWindow.SetAsLastSibling();
        }
        private void OnConfirmEvent()
        {
            askWindow.SetWindowActive(false);
            confirmCallback?.Invoke();
        }
        private void OnCancelEvent()
        {
            askWindow.SetWindowActive(false);
            cancelCallback?.Invoke();
        }
        public void End()
        {

        }
    }
}