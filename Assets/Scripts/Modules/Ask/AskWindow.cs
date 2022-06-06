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
            btn_confirm.onClick.AddListener(() => { SendEventMsg("OnConfirmEvent"); });
            btn_cancel.onClick.AddListener(() => { SendEventMsg("OnCancelEvent"); });
        }

        protected override void OpenWindow()
        {
            base.OpenWindow();

        }

        protected override void CloseWindow()
        {
            base.CloseWindow();

        }

        public void SetWindowInfo(string tips)
        {
            tipsText.text = tips;
        }
    }
}
