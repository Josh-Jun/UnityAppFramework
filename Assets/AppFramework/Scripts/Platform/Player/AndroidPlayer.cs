using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class AndroidPlayer : PlatformManager
    {
        private const string AppMainPackage = "com.unity3d.player.UnityPlayer";
        private const string AppToolsPackage = "com.debug.tools.AndroidHelper";
        public AndroidPlayer()
        {
            JavaObject(AppToolsPackage).CallStatic("init", MainJavaObject());
        }
        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "Android";
        public override string PlatformName
        {
            get { return Root.AppConfig.TargetPackage == TargetPackage.Mobile ? "android" : Root.AppConfig.TargetPackage.ToString().ToLower(); }
        } 
        public override string GetDataPath(string folder)
        {
            return $"file://{Application.persistentDataPath}/{folder}";
        }
        public override void InstallApp(string appPath)
        {
            string path = appPath.Replace("file://", "");
#if UNITY_ANDROID
            JavaObject(AppToolsPackage).CallStatic("installApp", path);
#endif
        }
        public override void SavePhoto(string imagePath)
        {
            string path = imagePath.Replace("file://", "");
#if UNITY_ANDROID
            JavaObject(AppToolsPackage).CallStatic("savePhoto", path);
#endif
        }
        public override string GetAppData(string key)
        {
#if UNITY_ANDROID
            return JavaObject(AppToolsPackage).CallStatic<string>("getAppData", key);
#else
            return "";
#endif
        }
        public override void QuitUnityPlayer()
        {
#if UNITY_ANDROID
        JavaObject(AppToolsPackage).CallStatic("quitUnityActivity");
#endif
        }
        private AndroidJavaObject MainJavaObject()
        {
            return JavaObject(AppMainPackage).GetStatic<AndroidJavaObject>("currentActivity");
        }
        private AndroidJavaObject JavaObject(string packageName)
        {
            return new AndroidJavaClass(packageName);
        }
    }
}
