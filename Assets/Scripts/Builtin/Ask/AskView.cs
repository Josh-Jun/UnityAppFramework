/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 弹窗功能(View) - 1,确认弹窗  2,提示弹窗
 * ===============================================
 * */
using System;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace App.Modules.Ask
{
    [ViewOf(ViewMold.UI2D, AssetPath.AskView)]
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

        public void Init()
        {
        }

        public void SetTips(string tips, float time)
        {
            if (tipsPanel.activeSelf) return;
            tipsPanel.SetActive(true);
            tipsText.text = tips;
            TimeTaskMaster.Instance.AddTimeTask(CloseView, time);
        }
        public void SetViewInfo(string content, Action confirm_callback, Action cancel_callback, string confirm = null, string cancel = null)
        {
            askPanel.SetActive(true);
            askText.text = content;
            confirm_text.text = string.IsNullOrEmpty(confirm) ? "确定" : confirm;
            cancel_text.text = string.IsNullOrEmpty(cancel) ? "取消" : cancel;
            btn_confirm.onClick.RemoveAllListeners();
            btn_cancel.onClick.RemoveAllListeners();
            btn_confirm.onClick.AddListener(() => { OnClickEvent(confirm_callback); });
            btn_cancel.onClick.AddListener(() => { OnClickEvent(cancel_callback); });
        }
        private void OnClickEvent(Action callback)
        {
            CloseView();
            callback?.Invoke();
        }
    }
}
