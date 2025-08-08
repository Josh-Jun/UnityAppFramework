/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 进度条场景(View) - 加载场景过渡功能
 * ===============================================
 * */

using System;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

namespace App.Modules
{
    [ViewOf(ViewMold.UI2D, AssetPath.LoadingView, false, 2)]
    public class LoadingView : ViewBase
    {
        private Slider loadingSlider;
        private TextMeshProUGUI loadingText;
        protected override void InitView()
        {
            base.InitView();
            loadingSlider = this.FindComponent<Slider>("LoadingSlider");
            loadingText = transform.GetComponentInChildren<TextMeshProUGUI>();;
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();

        }
        
        public void SetLoadingSliderValue(float value, Action callback = null)
        {
            loadingSlider.DOKill();
            var duration = value >= 1 ? 0.5f : 5f;
            var endValue = value >= 1 ? 1f : 0.9f;
            loadingSlider.DOValue(endValue, duration).SetEase(Ease.Linear).OnComplete(() => { callback?.Invoke(); });
            if(loadingText != null)
            {
                loadingText.text = $"{loadingSlider.value * 100:F2}%";
            }
        }
    }
}