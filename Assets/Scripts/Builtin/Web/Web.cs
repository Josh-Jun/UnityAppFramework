/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年5月9 15:8
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Web : MonoBehaviour
{
    #if UNITY_ANDROID || UNITY_IOS
    private UniWebView web;
    #endif
    private const string WEB_EVENT_ARG_CONFIG_PATH = "Launcher/WebEventArgConfig";
    private CrossAssemblyEventArgConfig WebEventArgConfig;
    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        web = GetComponentInChildren<UniWebView>();
#endif
        WebEventArgConfig = Resources.Load<CrossAssemblyEventArgConfig>(WEB_EVENT_ARG_CONFIG_PATH);
        WebEventArgConfig.AddListener(LoadWebHtml);
    }
    private void LoadWebHtml(CrossAssemblyEventDataBase dataBase)
    {
        if(dataBase is not WebEventData data) return;
#if UNITY_ANDROID || UNITY_IOS
        web.LoadHTMLString(data.html, "text/html");
        web.Show();
#endif
    }
}
