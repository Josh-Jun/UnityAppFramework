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
    [ViewOf("Loading", ViewMold.UI2D, AssetPath.LoadingView, false, 2)]
    public class LoadingView : ViewBase
    {
		private TextMeshProUGUI progresstextmeshprougui;
		private Slider loadingsliderslider;

		public TextMeshProUGUI ProgressTextMeshProUGUI { get { return progresstextmeshprougui; } }
		public Slider LoadingSliderSlider { get { return loadingsliderslider; } }

        protected override void InitView()
        {
            base.InitView();
			progresstextmeshprougui = this.FindComponent<TextMeshProUGUI>("LoadingSlider/Fill Area/Fill/Progress");
			loadingsliderslider = this.FindComponent<Slider>("LoadingSlider");

        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
			LoadingSliderSlider.onValueChanged.AddListener((arg) => { SendEventMsg("LoadingSliderSliderEvent", arg); });

        }
    }
}
