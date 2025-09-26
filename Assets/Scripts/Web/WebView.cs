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
    [ViewOf("Web", ViewMold.UI2D, AssetPath.WebView, false, 4)]
    public class WebView : ViewBase
    {
		private RectTransform webpanelrecttransform;
		private TextMeshProUGUI webtitletextmeshprougui;
		private Button closewebbutton;

		public RectTransform WebPanelRectTransform { get { return webpanelrecttransform; } }
		public TextMeshProUGUI WebTitleTextMeshProUGUI { get { return webtitletextmeshprougui; } }
		public Button CloseWebButton { get { return closewebbutton; } }

        protected override void InitView()
        {
            base.InitView();
			webpanelrecttransform = this.FindComponent<RectTransform>("WebPanel");
			webtitletextmeshprougui = this.FindComponent<TextMeshProUGUI>("WebTitle");
			closewebbutton = this.FindComponent<Button>("CloseWeb");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
			CloseWebButton.onClick.AddListener(() => { SendEventMsg("CloseWebButtonEvent"); });

        }
    }
}
