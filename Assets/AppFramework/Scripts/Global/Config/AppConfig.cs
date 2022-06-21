using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AppConfig", menuName = "App/AppConfig")]
[Serializable]
public class AppConfig : ScriptableObject
{
    [Header("App Config")]

    [Tooltip("App调试模式")]
    public bool IsDebug;
    [Tooltip("App服务器是否生产服")]
    public bool IsProServer;
    [Tooltip("App是否热更")]
    public bool IsHotfix;
    [Tooltip("App是否加载AB包")]
    public bool IsLoadAB;
    [Tooltip("App运行XLua脚本")]
    public bool RunXLua;
    [Tooltip("App运行帧频，默认60帧")]
    public int AppFrameRate = 60;
    [Tooltip("App安卓包不同渠道")]
    public TargetPackage TargetPackage;
    [Tooltip("App原生")]
    public bool NativeApp;
}