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

    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        web = GetComponentInChildren<UniWebView>();
#endif
    }
    private void LoadWebHtml(string html)
    {
#if UNITY_ANDROID || UNITY_IOS
        web.LoadHTMLString(html, "text/html");
        web.Show();
#endif
    }
}
