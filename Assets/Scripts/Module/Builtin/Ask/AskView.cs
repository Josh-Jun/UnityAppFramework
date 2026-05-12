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
    [ViewOf("Ask", ViewMold.UI2D, AssetPath.AskView, false, 4)]
    public class AskView : ViewBase
    {
		public RectTransform BackgroundRectTransform;

        protected override void InitView()
        {
            base.InitView();
			BackgroundRectTransform = this.FindComponent<RectTransform>("Background");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
    }
}
