using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ask
{
    public class AskWindow : WindowBase
    {
        private Text tipsText;
        private Button btn_confirm;
        private Button btn_cancel;
        protected override void InitWindow()
        {
            base.InitWindow();
            tipsText = this.FindComponent<Text>("AskPanel/TipsText");
            btn_confirm = this.FindComponent<Button>("AskPanel/BtnConfirm");
            btn_cancel = this.FindComponent<Button>("AskPanel/BtnCancel");
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
        }

        protected override void OpenWindow()
        {
            base.OpenWindow();

        }

        protected override void CloseWindow()
        {
            base.CloseWindow();

        }

        public void SetWindowInfo(string tips, Action confirm_callback = null, Action cancel_callback = null)
        {
            tipsText.text = tips;
            btn_confirm.onClick.AddListener(() => { OnClickEvent(btn_confirm, confirm_callback); });
            btn_cancel.onClick.AddListener(() => { OnClickEvent(btn_cancel, cancel_callback); });
        }
        private void OnClickEvent(Button btn, Action callback)
        {
            btn.onClick.RemoveAllListeners();
            SetWindowActive(false);
            callback?.Invoke();
        }
    }
}
