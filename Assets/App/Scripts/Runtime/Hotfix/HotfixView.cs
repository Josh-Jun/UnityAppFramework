/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月9 10:47
 * function    :
 * ===============================================
 * */

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.InteropServices;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace App.Runtime.Hotfix
{
    public class HotfixView : MonoBehaviour
    {
        public bool ShowAgree = true;
        #region Hotfix

        private Slider _slider;
        private TextMeshProUGUI _text;
        private TextMeshProUGUI _progressText;
        private float _min;

        #endregion

        #region Agree

        private const string Agreement = "ARGEEMENT";
        private Action _callback;
        private GameObject _agree;
        private TextMeshProUGUI _connect;
        private Button _agreeButton;
        private Button _unAgreeButton;

        private GameObject _agreePanel;
        private TextMeshProUGUI _agreeTitle;
        private TextMeshProUGUI _agreeText;

        #endregion

        private void Awake()
        {
            #region Hotfix
            _slider = transform.Find("Slider").GetComponent<Slider>();
            _text = transform.Find("Slider/Text").GetComponent<TextMeshProUGUI>();
            _progressText = transform.Find("Slider/Fill Area/Fill/Progress").GetComponent<TextMeshProUGUI>();
            if (_slider.gameObject.activeSelf)
                _slider.gameObject.SetActive(false);
            _min = _slider.value;
            #endregion

            #region Agree
            _agree = transform.Find("Agree").gameObject;
            _connect = _agree.transform.Find("Connect").GetComponent<TextMeshProUGUI>();
            _agreeButton = _agree.transform.Find("Agree").GetComponent<Button>();
            _unAgreeButton = _agree.transform.Find("UnAgree").GetComponent<Button>();
            _unAgreeButton.onClick.AddListener(() =>
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
            _agreeButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt(Agreement, 1);
                _callback?.Invoke();
            });
            var trigger = _connect.gameObject.AddComponent<EventTrigger>();
            var callback = new UnityAction<BaseEventData>(OnTextLinkClick);
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);

            if (_agree.activeSelf)
                _agree.SetActive(false);

            _agreePanel = transform.Find("AgreePanel").gameObject;
            _agreeTitle = _agreePanel.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            _agreeText = _agreePanel.transform.Find("ScrollView/Viewport/Content/Text").GetComponent<TextMeshProUGUI>();

            if (_agreePanel.activeSelf)
                _agreePanel.SetActive(false);

            #endregion
        }

        public void Startup(Action callback)
        {
            if (PlayerPrefs.HasKey(Agreement) || !ShowAgree)
            {
                callback?.Invoke();
                return;
            }
            _agree.SetActive(true);
            _callback = callback;
        }

        #region Hotfix
        public void SetDownloadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
        {
            if (!_slider.gameObject.activeSelf)
            {
                _slider.gameObject.SetActive(true);
            }
            var progress = currentDownloadBytes / (float)totalDownloadBytes;
            _slider.value = Mathf.Clamp(progress, _min, 1);
            _progressText.text = $"{(progress * 100):00}%";
            _text.text = $"{(currentDownloadBytes / 1048576f):F}M/{(totalDownloadBytes / 1048576f):F}M\n{currentDownloadCount}/{totalDownloadCount}";
        }
        #endregion

        #region Agree

        private void OnTextLinkClick(BaseEventData eventData)
        {
            if (eventData is not PointerEventData pointerEventData) return;
            // If you are not in a Canvas using Screen Overlay, put your camera instead of null
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_connect, pointerEventData.position, Camera.main);
            Debug.Log("linkIndex: " + linkIndex);
            if (linkIndex == -1) return;
            // was a link clicked?
            StartCoroutine(OpenAgreePanel(linkIndex));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param> 0:用户协议 1:隐私政策 <summary />
        private IEnumerator OpenAgreePanel(int index)
        {
            _agreePanel.SetActive(true);
            var title = index == 0 ? "用户协议" : "隐私政策";
            var api = index == 0 ? "/common/user_agreement" : "/common/privacy_policy";
            _agreeTitle.text = title;
            using var request = UnityWebRequest.Get(Global.HttpServer + api);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                _agreeText.text = request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Error fetching agreement: {request.error}");
                _agreeText.text = $"获取{title}失败，请检查网络连接后重试。";
            }
        }

        #endregion
    }
}