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

public class Hotfix : MonoBehaviour
{
    public bool isNeedRestart = false;
    
    private Slider slider;
    private TextMeshProUGUI text;
    private TextMeshProUGUI progressText;
    private GameObject updateCompletePanel;
    private Button btnConfirm;

    private float min;

    private void Awake()
    {
        slider = transform.Find("Slider").GetComponent<Slider>();
        text = transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>();
        progressText = transform.Find("Slider/Fill Area/Fill/Progress").GetComponent<TextMeshProUGUI>();
        updateCompletePanel = transform.Find("UpdateCompletePanel").gameObject;
        btnConfirm = transform.Find("UpdateCompletePanel/BtnConfirm").GetComponent<Button>();
        btnConfirm.onClick.AddListener(Application.Quit);
        if(updateCompletePanel.activeSelf)
            updateCompletePanel.SetActive(false);
        if(slider.gameObject.activeSelf)
            slider.gameObject.SetActive(false);
        min = slider.value;
    }

    public void SetUpdateCompletePanelActive(bool active)
    {
        if (!isNeedRestart) return;
        if(updateCompletePanel.activeSelf != active)
            updateCompletePanel.SetActive(active);
    }

    public void SetDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        if (!slider.gameObject.activeSelf)
        {
            slider.gameObject.SetActive(true);
        }
        var progress = currentDownloadBytes / (float)totalDownloadBytes;
        slider.value = Mathf.Clamp(progress, min, 1);
        progressText.text = $"{(progress * 100):F1}%";
        text.text = $"{(currentDownloadBytes / 1024f / 1024f):F}M/{(totalDownloadBytes / 1024f / 1024f):F}M  {currentDownloadCount}/{totalDownloadCount}";
    }
}
