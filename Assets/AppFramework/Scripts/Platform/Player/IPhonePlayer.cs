using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Platform
{
    public class IPhonePlayer : PlatformManager
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern string getAppData(string key);
        [DllImport("__Internal")]
        private static extern void showHostMainWindow(string msg);
#endif

        public override bool IsEditor()
        {
            return false;
        }
        public override string Name()
        {
            return "iOS";
        }
        public override string GetAppData(string key)
        {
#if UNITY_IPHONE
            return getAppData(key);
#else
            return null;
#endif
        }
        public override void QuitUnityPlayer()
        {
#if UNITY_IPHONE
            showHostMainWindow("");
#endif
        }
    }
}
