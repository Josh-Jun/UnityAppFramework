using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ask
{
    public class AskWindow : WindowBase
    {
        private GameObject askPanel;
        private Text askText;
        private Button btn_confirm;
        private Button btn_cancel;

        private GameObject tipsPanel;
        private Text tipsText;
        protected override void InitWindow()
        {
            base.InitWindow();
            askPanel = this.FindGameObject("AskPanel");
            askText = askPanel.FindComponent<Text>("TipsText");
            btn_confirm = askPanel.FindComponent<Button>("BtnConfirm");
            btn_cancel = askPanel.FindComponent<Button>("BtnCancel");
            
            tipsPanel = this.FindGameObject("TipsPanel");
            tipsText = tipsPanel.FindComponent<Text>("TipsBg/TipsText");
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
        }

        protected override void OpenWindow()
        {
            base.OpenWindow();
            askPanel.SetActive(false);
            tipsPanel.SetActive(false);
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();
            askPanel.SetActive(false);
            tipsPanel.SetActive(false);
        }

        public void SetTips(string tips, float time)
        {
            if (tipsPanel.activeSelf) return;
            tipsPanel.SetActive(true);
            tipsText.text = tips;
            TimerTaskManager.Instance.AddTimeTask(() => { SetWindowActive(false); }, time);
        }
        public void SetWindowInfo(string tips, Action confirm_callback = null, Action cancel_callback = null)
        {
            askPanel.SetActive(true);
            askText.text = tips;
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
