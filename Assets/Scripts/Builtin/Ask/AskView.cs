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
    [ViewOf(ViewMold.UI2D, AssetPath.AskView, false, 2)]
    public class AskView : ViewBase
    {
		private RectTransform backgroundrecttransform;

		public RectTransform BackgroundRectTransform { get { return backgroundrecttransform; } }

        protected override void InitView()
        {
            base.InitView();
			backgroundrecttransform = this.FindComponent<RectTransform>("Background");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
    }
}
