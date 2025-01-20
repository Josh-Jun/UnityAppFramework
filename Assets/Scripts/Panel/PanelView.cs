/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月20 9:22
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

namespace Modules.Panel
{
    [ViewOf(ViewMold.UI2D, AssetPath.PanelView, false, 0)]
    public class PanelView : ViewBase
    {
		private RectTransform buttonRectTransform;
		private Image buttonImage;
		private Button buttonButton;

		public Image ButtonImage { get { return buttonImage; } }

        protected override void InitView()
        {
            base.InitView();
			buttonRectTransform = this.FindComponent<RectTransform>("Button");
			buttonImage = this.FindComponent<Image>("Button");
			buttonButton = this.FindComponent<Button>("Button");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
			buttonButton.onClick.AddListener(() => { SendEventMsg("ButtonButtonEvent"); });

        }

        protected override void OpenView()
        {
            base.OpenView();

        }

        protected override void CloseView()
        {
            base.CloseView();

        }
    }
}
