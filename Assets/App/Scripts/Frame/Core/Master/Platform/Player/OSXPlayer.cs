using UnityEngine;

namespace App.Core.Master
{
    public class OSXPlayer : PlatformMaster
    {
        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "StandaloneOSX";
        public override string PlatformName { get; } = "macos";
        public override int KeyboardHeight => 0;
        public override int GetNetSignal()
        {
            return 0;
        }
        
        public override void SendMsgToNative(string msg)
        {
            Log.I("SendMsgToNative", ("Data", msg));
        }
        
        public override void OpenAppSetting()
        {
            Log.I("OpenAppSetting MacOS");
        }

        public override void RequestUserPermission(string permission)
        {
            Log.I("RequestUserPermission", ("Permission", permission));
        }

        public override string GetDataPath(string folder)
        {
            return $"{Application.dataPath.Replace("Assets", "")}{folder}";
        }
        public override string GetAssetsPath(string folder)
        {
            return $"{Application.streamingAssetsPath}/{folder}";
        }
        public override void InstallApp(string appPath)
        {
            Log.I("InstallApp", ("AppPath", appPath));
        }
        public override void Vibrate()
        {
            Log.I("Vibrate MacOS");
        }
        public override void QuitUnityPlayer()
        {
            Log.I("Quit MacOS");
            Application.Quit();
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
