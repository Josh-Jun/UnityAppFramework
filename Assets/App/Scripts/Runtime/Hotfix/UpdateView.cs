/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 更新功能
 * ===============================================
 * */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace App.Runtime.Hotfix
{
    public class UpdateView : MonoBehaviour
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
        private Action onUpdateNow;

        private void Awake()
        {
            progressBarPanel = transform.Find("ProgressBarPanel").GetComponent<RectTransform>();
            slider_Progress = transform.Find("ProgressBarPanel/ProgressSlider").GetComponent<Slider>();
            text_Tips = transform.Find("ProgressBarPanel/TipsText").GetComponent<Text>();
            text_Speed = transform.Find("ProgressBarPanel/SpeedText").GetComponent<Text>();
            text_Progress = transform.Find("ProgressBarPanel/ProgressText").GetComponent<Text>();

            updateTipsPanel = transform.Find("UpdateTipsPanel").GetComponent<RectTransform>();
            text_Content = transform.Find("UpdateTipsPanel/Scroll View Tips/Viewport/ContentText").GetComponent<Text>();
            btn_NextTime = transform.Find("UpdateTipsPanel/NextTime").GetComponent<Button>();
            btn_UpdateNow = transform.Find("UpdateTipsPanel/UpdateNow").GetComponent<Button>();
        }
        private void Start()
        {
            btn_NextTime.onClick.AddListener(() => { Application.Quit(); });
            btn_UpdateNow.onClick.AddListener(() => { onUpdateNow?.Invoke(); });
        }
        public void Init(Action onUpdateNow)
        {
            updateTipsPanel.gameObject.SetActive(false);
            progressBarPanel.gameObject.SetActive(false);
            text_Tips.text = "";
            text_Speed.text = "";
            text_Progress.text = "";
            text_Content.text = "";
            btn_UpdateNow.onClick.RemoveAllListeners();
            this.onUpdateNow = onUpdateNow;
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
            text_Speed.text = $"{value:F2}M/S";
        }
        public void SetProgressText(float value, float total)
        {
            text_Progress.text = $"{value:F2}M/{total:F2}M";
        }
        public void SetProgressValue(float value)
        {
            slider_Progress.value = value;
        }
        public void SetUpdateTipsActive(bool active)
        {
            updateTipsPanel.gameObject.SetActive(active);
        }
        public void SetProgressBarActive(bool active)
        {
            progressBarPanel.gameObject.SetActive(active);
        }
    }
}