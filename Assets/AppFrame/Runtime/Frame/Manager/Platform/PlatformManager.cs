using UnityEngine;

namespace AppFrame.Manager
{
    public abstract class PlatformManager
    {
        private static PlatformManager _instance;

        public static PlatformManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.WindowsPlayer:
                            _instance = new WindowPlayer();
                            break;
                        case RuntimePlatform.IPhonePlayer:
                            _instance = new IPhonePlayer();
                            break;
                        case RuntimePlatform.Android:
                            _instance = new AndroidPlayer();
                            break;
                        case RuntimePlatform.WindowsEditor:
                            _instance = new EditorPlayer();
                            break;
                        case RuntimePlatform.OSXEditor:
                            _instance = new EditorPlayer();
                            break;
                        default:
                            break;
                    }
                }

                return _instance;
            }
        }

        public abstract bool IsEditor { get; }
        public abstract string Name { get; }
        public abstract string PlatformName { get; }
        public abstract int GetNetSignal();
        public abstract void Vibrate();
        public abstract void RequestUserPermission(string permission);
        public abstract void OpenAppSetting();
        public abstract string GetDataPath(string folder);
        public abstract string GetAssetsPath(string folder);
        public abstract void InstallApp(string appPath);
        public abstract void SavePhoto(string imagePath);
        public abstract string GetAppData(string key);
        public abstract void QuitUnityPlayer();
    }
}