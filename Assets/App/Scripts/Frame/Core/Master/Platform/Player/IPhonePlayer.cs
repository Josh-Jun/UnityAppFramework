using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace App.Core.Master
{
    public class IPhonePlayer : PlatformMaster
    {
#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern IntPtr GetNativeData(IntPtr key);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void ShowHostMainWindow(IntPtr msg);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void NativeVibrate();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void OpenNativeSettings();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool HasNativeUserAuthorizedPermission(IntPtr permission);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void RequestNativeUserPermission(IntPtr permission);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void ReceiveUnityMsg(IntPtr msg);
#endif

        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "iOS";
        public override string PlatformName { get; } = "ios";

        public override int KeyboardHeight
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                if(!TouchScreenKeyboard.visible) return 0;
                return (int)TouchScreenKeyboard.area.height;
#endif
                return 0;
            }
        }

        public IPhonePlayer()
        {
            PlatformMsgReceiver.Instance.Init();
        }
        
        public override void SendMsgToNative(string msg)
        {
            Log.I("SendMsgToNative", ("Data", msg));
#if UNITY_IOS && !UNITY_EDITOR
            ReceiveUnityMsg(Marshal.StringToHGlobalAnsi(msg));
#endif
        }
        public override void OpenAppSetting()
        {
            Log.I("OpenAppSetting IPhone");
#if UNITY_IOS && !UNITY_EDITOR
            OpenNativeSettings();
#endif
        }

        public override void RequestUserPermission(string permission)
        {
            Log.I("RequestUserPermission", ("Permission", permission));
#if UNITY_IOS && !UNITY_EDITOR
            if(!HasNativeUserAuthorizedPermission(Marshal.StringToHGlobalAnsi(permission)))
            {
                RequestNativeUserPermission(Marshal.StringToHGlobalAnsi(permission));
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
            NativeVibrate();
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
            IntPtr ptr = GetNativeData(Marshal.StringToHGlobalAnsi(key));
            string result = Marshal.PtrToStringAnsi(ptr);
            // 这里如果 native 端有分配内存，记得释放
            return result;
#else
            return null;
#endif
        }
        public override void QuitUnityPlayer()
        {
            Log.I("Quit IPhone");
#if UNITY_IOS && !UNITY_EDITOR
            ShowHostMainWindow(Marshal.StringToHGlobalAnsi(""));
#endif
        }
    }
}
