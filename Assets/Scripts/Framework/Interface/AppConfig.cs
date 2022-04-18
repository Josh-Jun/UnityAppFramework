using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AppConfig", menuName = "App/AppConfig")]
[Serializable]
public class AppConfig : ScriptableObject
{
    [Header("App Config")]

    [Tooltip("App的调试模式")]
    public bool IsDebug;//是否打印Log
    [Tooltip("App服务器是否生产服务器")]
    public bool IsProServer;//是否测试服生产服
    [Tooltip("App是否热更")]
    public bool IsHotfix;//是否热更
    [Tooltip("App是否加载AB包")]
    public bool IsLoadAB;//是否加载AB
    [Tooltip("App是否运行XLua脚本")]
    public bool RunXLua;//是否热更
    [Tooltip("App的运行帧频，默认30帧")]
    public int AppFrameRate = 30;//App的运行帧频
    [Tooltip("App安卓包不同渠道")]
    public ApkTarget ApkTarget;//App安卓包不同渠道
}