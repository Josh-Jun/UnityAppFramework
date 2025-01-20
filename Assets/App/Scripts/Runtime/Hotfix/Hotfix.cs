/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月9 10:47
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hotfix : MonoBehaviour
{
    private Slider slider;
    private Text text;
    private GameObject updateCompletePanel;
    private Button btnConfirm;

    private void Awake()
    {
        slider = transform.Find("Slider").GetComponent<Slider>();
        text = transform.Find("Slider/Text").GetComponent<Text>();
        updateCompletePanel = transform.Find("UpdateCompletePanel").gameObject;
        btnConfirm = transform.Find("UpdateCompletePanel/BtnConfirm").GetComponent<Button>();
        btnConfirm.onClick.AddListener(Application.Quit);
        SetUpdateCompletePanelActive(false);
    }

    public void SetUpdateCompletePanelActive(bool active)
    {
        updateCompletePanel.SetActive(active);
    }

    public void SetDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        var progress = currentDownloadBytes / (float)totalDownloadBytes;
        slider.value = progress;
        text.text = $"{(currentDownloadBytes / 1024f / 1024f):F}M/{(totalDownloadBytes / 1024f / 1024f):F}M  {currentDownloadCount}/{totalDownloadCount}";
    }
}
