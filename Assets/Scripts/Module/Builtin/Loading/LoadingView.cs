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
    [ViewOf("Loading", ViewMold.UI2D, AssetPath.LoadingView, false, 7)]
    public class LoadingView : ViewBase
    {
	    public TextMeshProUGUI ProgressTextMeshProUGUI;
	    public Slider LoadingSliderSlider;

        protected override void InitView()
        {
            base.InitView();
			ProgressTextMeshProUGUI = this.FindComponent<TextMeshProUGUI>("LoadingSlider/Fill Area/Fill/Progress");
			LoadingSliderSlider = this.FindComponent<Slider>("LoadingSlider");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
			LoadingSliderSlider.onValueChanged.AddListener((arg) => { SendEventMsg("LoadingSliderSliderEvent", arg); });

        }
    }
}
