using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AppFrame.Config
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "App/AppConfig")]
    [Serializable]
    public class AppConfig : ScriptableObject
    {
        [Header("App Config")] [Tooltip("App调试模式")]
        public bool IsDebug;

        [Tooltip("App服务器是否测试服")] public bool IsTestServer;
        [Tooltip("App资源加载模式")] public LoadAssetsMold LoadAssetsMold;
        [Tooltip("App运行帧频，默认60帧")] public int AppFrameRate = 60;
        [Tooltip("App安卓包不同渠道")] public TargetPackage TargetPackage;
        [Tooltip("App原生")] public bool NativeApp;
        [Tooltip("AB包打包加载方式")] public ABPipeline ABPipeline;
        [Tooltip("AB资源版本")] public string ResVersion;
        [Tooltip("UI参考分辨率")] public Vector2 UIReferenceResolution;
        [Tooltip("UI边距偏移量")] public MarginOffset UIOffset;
    }

    /// <summary>安卓包不同渠道</summary>
    public enum TargetPackage
    {
        Mobile = 0,
        XR = 1,
    }

    /// <summary>AB包打包加载方式</summary>
    public enum ABPipeline
    {
        Default = 0,
        Scriptable = 1,
    }

    /// <summary>资源加载类型</summary>
    public enum LoadAssetsMold
    {
        Native = 0, //本地资源
        Local = 1, //本地ab
        Remote = 2, //远端ab
    }

    /// <summary> 边缘偏移量 </summary>
    [Serializable]
    public struct MarginOffset
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;

        public MarginOffset(int left, int right, int top, int bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }
}