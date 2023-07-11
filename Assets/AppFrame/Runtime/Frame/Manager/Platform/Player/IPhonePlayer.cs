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
        private static extern string GetAppData(string key);
        [DllImport("__Internal")]
        private static extern void ShowHostMainWindow(string msg);
        [DllImport("__Internal")]
        private static extern void SavePhoto(string path);
        [DllImport("__Internal")]
        private static extern void Vibrate();
        [DllImport("__Internal")]
        private static extern void OpenAppSettings();
        [DllImport("__Internal")]
        private static extern bool HasUserAuthorizedPermission(string permission);
        [DllImport("__Internal")]
        private static extern void RequestUserPermission(string permission);
#endif

        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "iOS";
        public override string PlatformName { get; } = "ios";

        public IPhonePlayer()
        {
            PlatformMsgReceiver.Instance.Init();
        }
        
        public override void OpenAppSetting()
        {
#if UNITY_IPHONE
            OpenAppSettings();
#endif
        }

        public override void RequestUserPermission(string permission)
        {
            
#if UNITY_IPHONE
            if(!HasUserAuthorizedPermission(permission))
            {
                RequestUserPermission(permission);
            }
#endif
        }

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
        public override void Vibrate()
        {
#if UNITY_IPHONE
            Vibrate();
#endif
        }
        public override void SavePhoto(string imagePath)
        {
#if UNITY_IPHONE
            SavePhoto(imagePath);
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
            return GetAppData(key);
#else
            return null;
#endif
        }
        public override void QuitUnityPlayer()
        {
#if UNITY_IPHONE
            ShowHostMainWindow("");
#endif
        }
    }
}
