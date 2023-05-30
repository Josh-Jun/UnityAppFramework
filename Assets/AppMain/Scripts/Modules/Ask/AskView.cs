using System;
using System.Collections;
using System.Collections.Generic;
using AppFrame.Tools;
using AppFrame.View;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Ask
{
    public class AskView : ViewBase
    {
        private GameObject askPanel;
        private Text askText;
        private Button btn_confirm;
        private Button btn_cancel;
        private Text confirm_text;
        private Text cancel_text;

        private GameObject tipsPanel;
        private Text tipsText;
        protected override void InitView()
        {
            base.InitView();
            askPanel = this.FindGameObject("AskPanel");
            askText = askPanel.FindComponent<Text>("TipsText");
            btn_confirm = askPanel.FindComponent<Button>("BtnConfirm");
            btn_cancel = askPanel.FindComponent<Button>("BtnCancel");
            confirm_text = askPanel.FindComponent<Text>("BtnConfirm/Text");
            cancel_text = askPanel.FindComponent<Text>("BtnCancel/Text");
            
            tipsPanel = this.FindGameObject("TipsPanel");
            tipsText = tipsPanel.FindComponent<Text>("TipsBg/TipsText");
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
        }

        protected override void OpenView()
        {
            base.OpenView();
            askPanel.SetActive(false);
            tipsPanel.SetActive(false);
        }

        protected override void CloseView()
        {
            base.CloseView();
            askPanel.SetActive(false);
            tipsPanel.SetActive(false);
        }

        public void SetTips(string tips, float time)
        {
            if (tipsPanel.activeSelf) return;
            tipsPanel.SetActive(true);
            tipsText.text = tips;
            TimeTaskManager.Instance.AddTimeTask(() => { SetViewActive(false); }, time);
        }
        public void SetViewInfo(AskData askData)
        {
            askPanel.SetActive(true);
            askText.text = askData.content;
            if (!string.IsNullOrEmpty(askData.confirm))
            {
                confirm_text.text = askData.confirm;
            }
            if (!string.IsNullOrEmpty(askData.cancel))
            {
                cancel_text.text = askData.cancel;
            }
            btn_confirm.onClick.AddListener(() => { OnClickEvent(btn_confirm, askData.confirm_callback); });
            btn_cancel.onClick.AddListener(() => { OnClickEvent(btn_cancel, askData.cancel_callback); });
        }
        private void OnClickEvent(Button btn, Action callback)
        {
            btn.onClick.RemoveAllListeners();
            SetViewActive(false);
            callback?.Invoke();
        }
    }
}
