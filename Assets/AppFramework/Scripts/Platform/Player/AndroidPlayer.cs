using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class AndroidPlayer : PlatformManager
    {
        public override bool IsEditor()
        {
            return false;
        }
        public override string Name()
        {
            return "Android";
        }
        public override string GetAppData(string key)
        {
#if UNITY_ANDROID
            return MainJavaObject().Call<string>("getAppData", key);
#else
            return "";
#endif
        }
        public override void QuitUnityPlayer()
        {
#if UNITY_ANDROID
            MainJavaObject().Call("quitUnityActivity");
#endif
        }
        private AndroidJavaObject MainJavaObject()
        {
            return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        }
        private AndroidJavaObject JavaObject(string packageName)
        {
            return new AndroidJavaClass(packageName);
        }
    }
}
