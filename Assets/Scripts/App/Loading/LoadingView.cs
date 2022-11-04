using System;
using System.Collections;
using System.Collections.Generic;
using AppFramework.Tools;
using AppFramework.View;
using UnityEngine;
using UnityEngine.UI;

namespace App.Loading
{
    public class LoadingView : ViewBase
    {
        private Slider loadingSlider;
        protected override void InitView()
        {
            base.InitView();
            loadingSlider = this.FindComponent<Slider>("LoadingSlider");
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }

        protected override void OpenView()
        {
            base.OpenView();
        }

        protected override void CloseView()
        {
            base.CloseView();

        }

        public void SetLoadingSliderValue(float value)
        {
            loadingSlider.value = value;
        }
    }
}
