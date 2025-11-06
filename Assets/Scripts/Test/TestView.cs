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
    [ViewOf("Test", ViewMold.UI2D, AssetPath.TestView, false, 3)]
    public class TestView : ViewBase
    {
		public RawImage RawImageRawImage;

        protected override void InitView()
        {
            base.InitView();
			RawImageRawImage = this.FindComponent<RawImage>("RawImage");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
    }
}
