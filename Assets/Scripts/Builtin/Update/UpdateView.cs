/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 更新功能(View) - 1,资源更新 2,应用更新
 * ===============================================
 * */

using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.Modules.Update
{
    public class UpdateView : ViewBase
    {
        private RectTransform progressBarPanel;
        private Slider slider_Progress;
        private TextMeshProUGUI text;
        private TextMeshProUGUI text_Progress;

        private RectTransform updateTipsPanel;
        private Text text_Content;
        private Button btn_NextTime;
        private Button btn_UpdateNow;
        protected override void InitView()
        {
            base.InitView();

            progressBarPanel = this.FindComponent<RectTransform>("ProgressBarPanel");
            slider_Progress = this.FindComponent<Slider>("ProgressBarPanel/ProgressSlider");
            text = this.FindComponent<TextMeshProUGUI>("ProgressBarPanel/ProgressSlider/Text");
            text_Progress = this.FindComponent<TextMeshProUGUI>("ProgressBarPanel/ProgressSlider/Fill Area/Fill/Progress");

            updateTipsPanel = this.FindComponent<RectTransform>("UpdateTipsPanel");
            text_Content = this.FindComponent<Text>("UpdateTipsPanel/Scroll View Tips/Viewport/ContentText");
            btn_NextTime = this.FindComponent<Button>("UpdateTipsPanel/NextTime");
            btn_UpdateNow = this.FindComponent<Button>("UpdateTipsPanel/UpdateNow");
        }
        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            btn_NextTime.onClick.AddListener(Application.Quit);
            btn_UpdateNow.onClick.AddListener(() => { SendEventMsg("UpdateNow"); });
        }

        public void Reset()
        {
            updateTipsPanel.SetGameObjectActive(false);
            progressBarPanel.SetGameObjectActive(false);
            text.text = "";
            text_Progress.text = "";
            text_Content.text = "";
        }
        
        public void SetContentText(string value)
        {
            text_Content.text = value;
        }
        public void SetDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            var progress = currentDownloadBytes / (float)totalDownloadBytes;
            slider_Progress.value = Mathf.Clamp(progress, 0, 1);
            text_Progress.text = $"{progress * 100:F2}%";
            text.text = $"{currentDownloadBytes / 1048576f:F2}M/{totalDownloadBytes / 1048576f:F2}M\n{currentDownloadCount}/{totalDownloadCount}";
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