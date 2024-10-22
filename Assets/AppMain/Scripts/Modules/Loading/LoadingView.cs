/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 进度条场景(View) - 加载场景过渡功能
 * ===============================================
 * */

using AppFrame.Attribute;
using AppFrame.Enum;
using AppFrame.Tools;
using AppFrame.View;
using UnityEngine.UI;

namespace Modules.Loading
{
    [ViewOf(ViewMold.UI2D, "Loading", false, 1)]
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
