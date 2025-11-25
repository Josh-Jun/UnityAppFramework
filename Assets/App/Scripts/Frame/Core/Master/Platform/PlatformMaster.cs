using UnityEngine;

namespace App.Core.Master
{
    public abstract class PlatformMaster
    {
        private static PlatformMaster _instance;

        public static PlatformMaster Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = Application.platform switch
                {
                    RuntimePlatform.OSXPlayer => new OSXPlayer(),
                    RuntimePlatform.WindowsPlayer => new WindowPlayer(),
                    RuntimePlatform.IPhonePlayer => new IPhonePlayer(),
                    RuntimePlatform.Android => new AndroidPlayer(),
                    RuntimePlatform.WindowsEditor or RuntimePlatform.OSXEditor => new EditorPlayer(),
                    _ => _instance
                };

                return _instance;
            }
        }

        public abstract bool IsEditor { get; }
        public abstract string Name { get; }
        public abstract string PlatformName { get; }
        public abstract void SendMsgToNative(string msg);
        public abstract int GetNetSignal();
        public abstract void Vibrate();
        public abstract void RequestUserPermission(string permission);
        public abstract void OpenAppSetting();
        public abstract string GetDataPath(string folder);
        public abstract string GetAssetsPath(string folder);
        public abstract void InstallApp(string appPath);
        public abstract string GetAppData(string key);
        public abstract void QuitUnityPlayer();
    }
}