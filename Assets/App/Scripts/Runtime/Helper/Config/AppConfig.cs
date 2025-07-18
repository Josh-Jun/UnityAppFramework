using System;
using UnityEngine;
using YooAsset;

namespace App.Runtime.Helper
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "App/AppConfig")]
    [Serializable]
    public class AppConfig : ScriptableObject
    {
        [Header("App Config")]
        [Space]
        [Tooltip("Log日志开关")]
        public bool EnableLog;
        [Tooltip("App开发环境")]
        public DevelopmentMold DevelopmentMold;
        [Tooltip("App资源加载模式")]
        public EPlayMode AssetPlayMode;
        [Tooltip("App运行帧频，默认60帧")]
        public int AppFrameRate = 60;
        [Tooltip("App渠道包")]
        public ChannelPackage ChannelPackage;
        [Tooltip("App原生")]
        public bool NativeApp;
        [Tooltip("CDN资源版本")]
        public string CDNVersion;
    }

    /// <summary>App渠道包</summary>
    public enum ChannelPackage
    {
        Default = 0,
        PicoXR = 1,
        Xiaomi = 2,
        Huawei = 3,
        Vivo = 4,
        Oppo = 5,
        Meizu = 6,
        Honor = 7,
    }

    /// <summary>开发环境</summary>
    public enum DevelopmentMold
    {
        Local = 0,
        Release = 1,
        Sandbox = 2,
        Test = 3,
    }
}