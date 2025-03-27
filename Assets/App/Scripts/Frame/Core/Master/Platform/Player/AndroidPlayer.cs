using App.Runtime.Helper;
using UnityEngine;
using UnityEngine.Android;

namespace App.Core.Master
{
    public class AndroidPlayer : PlatformMaster
    {
        private const string AppMainPackage = "com.unity3d.player.UnityPlayer";
        private const string AppToolsPackage = "com.debug.tools.AndroidHelper";
        private AndroidJavaObject MainJavaObject => JavaObject(AppMainPackage).GetStatic<AndroidJavaObject>("currentActivity");
        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "Android";
        public override string PlatformName => Global.AppConfig.ChannelPackage == ChannelPackage.Mobile ? "android" : Global.AppConfig.ChannelPackage.ToString().ToLower();

        public AndroidPlayer()
        {
            PlatformMsgReceiver.Instance.Init();
            JavaObject(AppToolsPackage).CallStatic("init", MainJavaObject);
        }

        private PermissionCallbacks _permissionCallbacks;
        private PermissionCallbacks PermissionCallbacks
        {
            get
            {
                if (_permissionCallbacks == null)
                {
                    _permissionCallbacks = new PermissionCallbacks();
                    _permissionCallbacks.PermissionGranted += OnGranted;
                    _permissionCallbacks.PermissionDenied += OnDenied;
                    _permissionCallbacks.PermissionDeniedAndDontAskAgain += OnDeniedAndDontAskAgain;
                }
                return _permissionCallbacks;
            }
        }
        //同意麦克风权限
        private void OnGranted(string permission)
        {
            PlatformMsgReceiver.Instance.AndroidPermissionCallbacks(permission, 1);
        }
        //拒绝麦克风权限
        private void OnDenied(string permission)
        {
            PlatformMsgReceiver.Instance.AndroidPermissionCallbacks(permission, 0);
        }
        //无法获取麦克风权限，打开设置
        private void OnDeniedAndDontAskAgain(string permission)
        {
            PlatformMsgReceiver.Instance.AndroidPermissionCallbacks(permission, -1);
        }

        public override void RequestUserPermission(string permission)
        {
            Log.I("RequestUserPermission", ("Permission", permission));
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission, PermissionCallbacks);
            }
#endif
        }

        public override void OpenAppSetting()
        {
            Log.I("OpenAppSetting Android");
#if UNITY_ANDROID && !UNITY_EDITOR
            JavaObject(AppToolsPackage).CallStatic("openAppSetting");
#endif
        }

        public override int GetNetSignal()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                return JavaObject(AppToolsPackage).CallStatic<int>("getMonetSignal");
            }
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                return JavaObject(AppToolsPackage).CallStatic<int>("getWiFiSignal");
            }
#endif
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
        public override void SendMsgToNative(string msg)
        {
            Log.I("SendMsgToNative", ("Data", msg));
#if UNITY_ANDROID && !UNITY_EDITOR
            JavaObject(AppToolsPackage).CallStatic("receiveUnityMsg", msg);
#endif
        }
        public override void InstallApp(string appPath)
        {
            Log.I("InstallApp", ("AppPath", appPath));
#if UNITY_ANDROID && !UNITY_EDITOR
            JavaObject(AppToolsPackage).CallStatic("installApp", appPath);
#endif
        }
        public override void Vibrate()
        {
            Log.I("Vibrate Android");
#if UNITY_ANDROID && !UNITY_EDITOR
            var mpattern = new long[] { 0, 150 };
            JavaObject(AppToolsPackage).CallStatic("vibrate", mpattern, -1);
#endif
        }
        public override void SavePhoto(string imagePath)
        {
            Log.I("SavePhoto", ("ImagePath", imagePath));
#if UNITY_ANDROID && !UNITY_EDITOR
            JavaObject(AppToolsPackage).CallStatic("savePhoto", imagePath);
#endif
        }
        public override string GetAppData(string key)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return JavaObject(AppToolsPackage).CallStatic<string>("getAppData", key);
#else
            return "";
#endif
        }
        public override void QuitUnityPlayer()
        {
            Log.I("Quit Android");
#if UNITY_ANDROID && !UNITY_EDITOR
        JavaObject(AppToolsPackage).CallStatic("quitUnityActivity");
#endif
        }
        private AndroidJavaObject JavaObject(string packageName)
        {
            return new AndroidJavaClass(packageName);
        }
    }
}
