/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月26 15:47
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
# if UNITY_EDITOR || UNITY_STANDALONE
using ZenFulcrum.EmbeddedBrowser;
#endif

namespace App.Modules
{
    [Serializable]
    public class WebData
    {
        public string title;
        public string url;
        public string html;
        public bool isClear;
    }

    [LogicOf("Web", AssetPath.Global)]
    public class WebLogic : EventBase, ILogic
    {
        private WebView View => ViewMaster.Instance.GetView<WebView>();
# if UNITY_EDITOR || UNITY_STANDALONE
        private Browser web;
#elif UNITY_ANDROID && !UNITY_EDITOR || UNITY_IOS && !UNITY_EDITOR
        private UniWebView web;
#endif
        public WebLogic()
        {
            AddEventMsg<object>("OpenWebView", OpenWebView);
            AddEventMsg("CloseWebView", CloseWebView);
            AddEventMsg("CloseWebButtonEvent", OnCloseWebButtonEvent);
        }

        #region Life Cycle

        public void Begin()
        {
        }

        public void End()
        {
        }

        public void AppPause(bool pause)
        {
        }

        public void AppFocus(bool focus)
        {
        }

        public void AppQuit()
        {
        }

        #endregion

        #region Logic

        private void OnCloseWebButtonEvent()
        {
            View.CloseView();
        }
        
        private void LoadWebView(WebData data)
        {
            View.WebTitleTextMeshProUGUI.text = data.title;
# if UNITY_EDITOR || UNITY_STANDALONE
            if (!web)
            {
                var prefab = AssetsMaster.Instance.LoadAssetSync<GameObject>(AssetPath.Browser);
                var go = Object.Instantiate(prefab, View.WebPanelRectTransform);
                web = go.GetComponent<Browser>();
            }
            
            if (!string.IsNullOrEmpty(data.url))
            {
                web.Url = data.url;
            }
            
            if (!string.IsNullOrEmpty(data.html))
            {
                web.LoadHTML(data.html);
            }
#elif UNITY_ANDROID && !UNITY_EDITOR || UNITY_IOS && !UNITY_EDITOR
            if (!web)
            {
                var prefab = AssetsMaster.Instance.LoadAssetSync<GameObject>(AssetPath.UniWebView);
                var go = Object.Instantiate(prefab, View.WebPanelRectTransform);
                web = go.GetComponent<UniWebView>();
            }

            if (!string.IsNullOrEmpty(data.url))
            {
                web.Load(data.url);
            }

            if (!string.IsNullOrEmpty(data.html))
            {
                web.LoadHTMLString(data.html, "text/html");
            }

            web.Show();
#endif
        }

        #endregion

        #region View Logic

        private void OpenWebView(object obj)
        {
            if (obj is WebData data)
            {
                LoadWebView(data);
                isClear = data.isClear;
            }
            else
            {
                Log.W("OpenWebView obj is not WebData");
            }
        }
        
        private bool isClear = false;

        private void CloseWebView()
        {
            if(!isClear) return;
            Object.Destroy(web.gameObject);
# if UNITY_EDITOR || UNITY_STANDALONE
            web.CookieManager.ClearAll();
#elif UNITY_ANDROID && !UNITY_EDITOR || UNITY_IOS && !UNITY_EDITOR
            web.CleanCache();
#endif
            web = null;
        }

        #endregion
    }
}