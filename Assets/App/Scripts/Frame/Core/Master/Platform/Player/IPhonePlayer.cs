using App.Core.Helper;
using UnityEngine;

namespace App.Core.Master
{
    public class IPhonePlayer : PlatformMaster
    {
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
#if UNITY_IOS && !UNITY_EDITOR
            iOS.ReceiveUnityMsg(msg);
#endif
        }
        public override void OpenAppSetting()
        {
            Log.I("OpenAppSetting IPhone");
#if UNITY_IOS && !UNITY_EDITOR
            iOS.OpenNativeSettings();
#endif
        }

        public override void RequestUserPermission(string permission)
        {
            Log.I("RequestUserPermission", ("Permission", permission));
#if UNITY_IOS && !UNITY_EDITOR
            if(!iOS.HasNativeUserAuthorizedPermission(permission))
            {
                iOS.RequestNativeUserPermission(permission);
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
#if UNITY_IOS && !UNITY_EDITOR
            iOS.NativeVibrate();
#endif
        }
        public override void InstallApp(string appPath)
        {
            Log.I("InstallApp", ("AppPath", appPath));
#if UNITY_IOS && !UNITY_EDITOR
            
#endif
        }
        public override string GetAppData(string key)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return iOS.GetNativeData(key);
#else
            return null;
#endif
        }
        public override void QuitUnityPlayer()
        {
            Log.I("Quit IPhone");
#if UNITY_IOS && !UNITY_EDITOR
            iOS.ShowHostMainWindow("");
#endif
        }
    }
}
