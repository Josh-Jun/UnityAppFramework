using AppFrame.Config;
using UnityEngine;
using UnityEngine.Android;

namespace AppFrame.Manager
{
    public class AndroidPlayer : PlatformManager
    {
        private const string AppMainPackage = "com.unity3d.player.UnityPlayer";
        private const string AppToolsPackage = "com.debug.tools.AndroidHelper";
        private AndroidJavaObject MainJavaObject
        {
            get {
                return JavaObject(AppMainPackage).GetStatic<AndroidJavaObject>("currentActivity");
            }
        }
        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "Android";
        public override string PlatformName
        {
            get { return Global.AppConfig.TargetPackage == TargetPackage.Mobile ? "android" : Global.AppConfig.TargetPackage.ToString().ToLower(); }
        } 
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
#if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission, PermissionCallbacks);
            }
#endif
        }

        public override void OpenAppSetting()
        {
            
#if UNITY_ANDROID
            JavaObject(AppToolsPackage).CallStatic("openAppSetting");
#endif
        }

        public override int GetNetSignal()
        {
#if UNITY_ANDROID
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
        public override void InstallApp(string appPath)
        {
#if UNITY_ANDROID
            JavaObject(AppToolsPackage).CallStatic("installApp", appPath);
#endif
        }
        public override void Vibrate()
        {
#if UNITY_ANDROID
            var mpattern = new long[] { 0, 150 };
            JavaObject(AppToolsPackage).CallStatic("vibrate", mpattern, -1);
#endif
        }
        public override void SavePhoto(string imagePath)
        {
#if UNITY_ANDROID
            JavaObject(AppToolsPackage).CallStatic("savePhoto", imagePath);
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
        private AndroidJavaObject JavaObject(string packageName)
        {
            return new AndroidJavaClass(packageName);
        }
    }
}
