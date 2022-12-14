using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AppFrame.Manager
{
    public class IPhonePlayer : PlatformManager
    {
#if UNITY_IPHONE
        [DllImport("__Internal")]
        private static extern string getAppData(string key);
        [DllImport("__Internal")]
        private static extern void showHostMainWindow(string msg);
        [DllImport("__Internal")]
        private static extern void savePhoto(string path);
#endif

        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "iOS";
        public override string PlatformName { get; } = "ios";
        public override int GetNetSignal()
        {
            return 0;
        }
        public override string GetDataPath(string folder)
        {
            return $"{Application.persistentDataPath}/{folder}";
        }
        public override string GetAssetsPath(string folder)
        {
            return $"{Application.streamingAssetsPath}/{folder}";
        }
        public override void SavePhoto(string imagePath)
        {
#if UNITY_IPHONE
            savePhoto(imagePath);
#endif
        }
        public override void InstallApp(string appPath)
        {
#if UNITY_IPHONE
            
#endif
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
