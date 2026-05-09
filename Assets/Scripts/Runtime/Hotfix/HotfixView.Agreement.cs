/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年5月9 14:34
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace App.Runtime.Hotfix
{
    public partial class HotfixView
    {
        private const string UserAgreementUrl = "https://ciyuanbanlv.oss-cn-beijing.aliyuncs.com/html/PrivacyAgree.html";
        private const string PrivacyPolicyUrl = "https://ciyuanbanlv.oss-cn-beijing.aliyuncs.com/html/PrivacyAgree.html";
        
        private GameObject _agree;
        private TextMeshProUGUI _connect;
        private Button _agreeButton;
        private Button _unAgreeButton;

        private GameObject _agreePanel;
        private TextMeshProUGUI _agreeTitle;
        private GameObject web;

        private Action agreeEvent;
        private void Init()
        {
            _agree = transform.Find("Agree").gameObject;
            _connect = _agree.transform.Find("Connect").GetComponent<TextMeshProUGUI>();
            _agreeButton = _agree.transform.Find("Agree").GetComponent<Button>();
            _unAgreeButton = _agree.transform.Find("UnAgree").GetComponent<Button>();
            _unAgreeButton.onClick.RemoveAllListeners();
            _unAgreeButton.onClick.AddListener(() =>
            {
                _agree.SetActive(false);
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
            _agreeButton.onClick.RemoveAllListeners();
            _agreeButton.onClick.AddListener(() =>
            {
                _agree.SetActive(false);
                PlayerPrefs.SetInt(Agreement, 1);
                agreeEvent?.Invoke();
            });
            var trigger = _connect.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();
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
            web =  _agreePanel.transform.Find("WebPanel/UniWeb").gameObject;

            if (_agreePanel.activeSelf)
                _agreePanel.SetActive(false);
        }

        private void ShowAgreePanelEvent(object obj)
        {
            Init();
            if (obj is not Action action) return;
            agreeEvent = action;
            _agree.SetActive(true);
        }
        
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
        
        private IEnumerator OpenAgreePanel(int index)
        {
            if (string.IsNullOrEmpty(UserAgreementUrl) || string.IsNullOrEmpty(PrivacyPolicyUrl)) yield break;
            _agreePanel.SetActive(true);
            var title = index == 0 ? "用户协议" : "隐私政策";
            var url = index == 0 ? UserAgreementUrl : PrivacyPolicyUrl;
            _agreeTitle.text = title;
            using var request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                web.SendMessage("LoadWebHtml", request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error fetching agreement: {request.error}");
            }
        }
        
    }
}
