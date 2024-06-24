using UnityEngine;

namespace AppFrame.Manager
{
    public class WindowPlayer : PlatformManager
    {
        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "StandaloneWindows";
        public override string PlatformName { get; } = "window";
        public override int GetNetSignal()
        {
            return 0;
        }
        
        public override void OpenAppSetting()
        {
            
        }

        public override void RequestUserPermission(string permission)
        {
            
        }

        public override string GetDataPath(string folder)
        {
            return $"{Application.dataPath.Replace("Assets", "")}{folder}";
        }
        public override string GetAssetsPath(string folder)
        {
            return $"{Application.streamingAssetsPath}/{folder}";
        }
        public override void SavePhoto(string imagePath)
        {
            Log.I("SavePhoto");
        }
        public override void InstallApp(string appPath)
        {
            
        }
        public override void Vibrate()
        {
            Log.I("Vibrate Window");
        }
        public override void QuitUnityPlayer()
        {
            Log.I("Quit Window");
            Application.Quit();
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
