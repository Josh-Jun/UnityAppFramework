using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Loading
{
    public class LoadingWindow : WindowBase
    {
        private Slider loadingSlider;
        protected override void InitWindow()
        {
            base.InitWindow();
            loadingSlider = this.FindComponent<Slider>("LoadingSlider");
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }

        protected override void OpenWindow()
        {
            base.OpenWindow();
        }

        protected override void CloseWindow()
        {
            base.CloseWindow();

        }

        public void SetLoadingSliderValue(float value)
        {
            loadingSlider.value = value;
        }
    }
}
