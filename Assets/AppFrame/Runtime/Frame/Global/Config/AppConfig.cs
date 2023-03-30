using System;
using AppFrame.Enum;
using UnityEngine;
using UnityEngine.Serialization;

namespace AppFrame.Config
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "App/AppConfig")]
    [Serializable]
    public class AppConfig : ScriptableObject
    {
        [Header("App Config")] 
        [Tooltip("App调试模式")] public bool IsDebug;
        [Tooltip("App服务器是否测试服")] public bool IsTestServer;
        [Tooltip("App资源加载模式")] public LoadAssetsMold LoadAssetsMold;
        [Tooltip("App运行帧频，默认60帧")] public int AppFrameRate = 60;
        [Tooltip("App安卓包不同渠道")] public TargetPackage TargetPackage;
        [Tooltip("App原生")] public bool NativeApp;
        [Tooltip("AB包打包加载方式")] public ABPipeline ABPipeline;
    }
}