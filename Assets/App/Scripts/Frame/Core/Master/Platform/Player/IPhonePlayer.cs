using UnityEngine;

namespace App.Core.Master
{
    public class IPhonePlayer : PlatformMaster
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetAppData(string key);
        [DllImport("__Internal")]
        private static extern void ShowHostMainWindow(string msg);
        [DllImport("__Internal")]
        private static extern void Vibrate();
        [DllImport("__Internal")]
        private static extern void OpenAppSettings();
        [DllImport("__Internal")]
        private static extern bool HasUserAuthorizedPermission(string permission);
        [DllImport("__Internal")]
        private static extern void RequestUserPermission(string permission);
        [DllImport("__Internal")]
        private static extern void ReceiveUnityMsg(string msg);
#endif

        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "iOS";
        public override string PlatformName { get; } = "ios";

        public IPhonePlayer()
        {
            PlatformMsgReceiver.Instance.Init();
        }
        
        public override void SendMsgToNative(string msg)
        {
            Log.I("SendMsgToNative", ("Data", msg));
#if UNITY_IPHONE && !UNITY_EDITOR
            ReceiveUnityMsg(msg);
#endif
        }
        public override void OpenAppSetting()
        {
            Log.I("OpenAppSetting IPhone");
#if UNITY_IPHONE && !UNITY_EDITOR
            OpenAppSettings();
#endif
        }

        public override void RequestUserPermission(string permission)
        {
            Log.I("RequestUserPermission", ("Permission", permission));
#if UNITY_IPHONE && !UNITY_EDITOR
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
            Log.I("Vibrate IPhone");
#if UNITY_IPHONE && !UNITY_EDITOR
            Vibrate();
#endif
        }
        public override void InstallApp(string appPath)
        {
            Log.I("InstallApp", ("AppPath", appPath));
#if UNITY_IPHONE && !UNITY_EDITOR
            
#endif
        }
        public override string GetAppData(string key)
        {
#if UNITY_IPHONE && !UNITY_EDITOR
            return GetAppData(key);
#else
            return null;
#endif
        }
        public override void QuitUnityPlayer()
        {
            Log.I("Quit IPhone");
#if UNITY_IPHONE && !UNITY_EDITOR
            ShowHostMainWindow("");
#endif
        }
    }
}
