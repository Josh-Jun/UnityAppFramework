/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月26 15:47
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.Modules
{
    [ViewOf("Web", ViewMold.UI2D, AssetPath.WebView, false, 3)]
    public class WebView : ViewBase
    {
		public RectTransform WebPanelRectTransform;
		public TextMeshProUGUI WebTitleTextMeshProUGUI;
		public Button CloseWebButton;

        protected override void InitView()
        {
            base.InitView();
			WebPanelRectTransform = this.FindComponent<RectTransform>("WebPanel");
			WebTitleTextMeshProUGUI = this.FindComponent<TextMeshProUGUI>("WebTitle");
			CloseWebButton = this.FindComponent<Button>("CloseWeb");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
			CloseWebButton.onClick.AddListener(() => { SendEventMsg("CloseWebButtonEvent"); });

        }
    }
}
