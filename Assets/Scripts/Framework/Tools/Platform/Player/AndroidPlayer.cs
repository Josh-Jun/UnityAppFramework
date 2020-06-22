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
        public override string CachePath()
        {
            return string.Format(@"{0}/", Application.persistentDataPath);
        }

        #region AndroidCallBack
        private AndroidJavaObject JavaObject()
        {
            return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        }
        private void CallJavaMethed(string methedName, params object[] args)
        {
            JavaObject().Call(methedName, args);
        }
        private T CallJavaMethed<T>(string methedName, params object[] args)
        {
            return (T)Convert.ChangeType(JavaObject().Call<T>(methedName, args), typeof(T));
        }
        private void CallJavaStaticMethed(string methedName, params object[] args)
        {
            JavaObject().CallStatic(methedName, args);
        }
        private T CallJavaStaticMethed<T>(string methedName, params object[] args)
        {
            return (T)Convert.ChangeType(JavaObject().CallStatic<T>(methedName, args), typeof(T));
        }
        #endregion
    }
}
