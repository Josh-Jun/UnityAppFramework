using AppFrame.Tools;
using AppFrame.View;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Update
{
    public class UpdateView : ViewBase
    {
        private RectTransform progressBarPanel;
        private Slider slider_Progress;
        private Text text_Tips;
        private Text text_Speed;
        private Text text_Progress;

        private RectTransform updateTipsPanel;
        private Text text_Content;
        private Button btn_NextTime;
        private Button btn_UpdateNow;
        protected override void InitView()
        {
            base.InitView();

            progressBarPanel = this.FindComponent<RectTransform>("ProgressBarPanel");
            slider_Progress = this.FindComponent<Slider>("ProgressBarPanel/ProgressSlider");
            text_Tips = this.FindComponent<Text>("ProgressBarPanel/TipsText");
            text_Speed = this.FindComponent<Text>("ProgressBarPanel/SpeedText");
            text_Progress = this.FindComponent<Text>("ProgressBarPanel/ProgressText");

            updateTipsPanel = this.FindComponent<RectTransform>("UpdateTipsPanel");
            text_Content = this.FindComponent<Text>("UpdateTipsPanel/Scroll View Tips/Viewport/ContentText");
            btn_NextTime = this.FindComponent<Button>("UpdateTipsPanel/NextTime");
            btn_UpdateNow = this.FindComponent<Button>("UpdateTipsPanel/UpdateNow");
        }
        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            btn_NextTime.onClick.AddListener(() => { Application.Quit(); });
            btn_UpdateNow.onClick.AddListener(() => { SendEventMsg("UpdateNow"); });
        }
        protected override void OpenView()
        {
            base.OpenView();
            
            updateTipsPanel.SetGameObjectActive(false);
            progressBarPanel.SetGameObjectActive(false);
            text_Tips.text = "";
            text_Speed.text = "";
            text_Progress.text = "";
            text_Content.text = "";
        }
        protected override void CloseView()
        {
            base.CloseView();

        }
        public void SetContentText(string value)
        {
            text_Content.text = value;
        }
        public void SetTipsText(string value)
        {
            text_Tips.text = value;
        }
        public void SetSpeedText(float value)
        {
            text_Speed.text = string.Format("{0}M/S", value.ToString("F2"));
        }
        public void SetProgressText(float value, float total)
        {
            text_Progress.text = string.Format("{0}M/{1}M", value.ToString("F2"), total.ToString("F2"));
        }
        public void SetProgressValue(float value)
        {
            slider_Progress.value = value;
        }
        public void SetUpdateTipsActive(bool active)
        {
            updateTipsPanel.SetGameObjectActive(active);
        }
        public void SetProgressBarActive(bool active)
        {
            progressBarPanel.SetGameObjectActive(active);
        }
    }
}