/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月9 10:47
 * function    :
 * ===============================================
 * */

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace App.Runtime.Hotfix
{
    public class HotfixView : MonoBehaviour
    {
        private Slider slider;
        private TextMeshProUGUI text;
        private TextMeshProUGUI progressText;

        private float min;

        private void Awake()
        {
            slider = transform.Find("Slider").GetComponent<Slider>();
            text = transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>();
            progressText = transform.Find("Slider/Fill Area/Fill/Progress").GetComponent<TextMeshProUGUI>();
            if (slider.gameObject.activeSelf)
                slider.gameObject.SetActive(false);
            min = slider.value;
        }

        public void SetDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            if (!slider.gameObject.activeSelf)
            {
                slider.gameObject.SetActive(true);
            }
            var progress = currentDownloadBytes / (float)totalDownloadBytes;
            slider.value = Mathf.Clamp(progress, min, 1);
            progressText.text = $"{(progress * 100):00}%";
            text.text = $"{(currentDownloadBytes / 1024f / 1024f):F}M/{(totalDownloadBytes / 1024f / 1024f):F}M\n{currentDownloadCount}/{totalDownloadCount}";
        }
    }
}