using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace App.Core.Master
{
    public class EditorPlayer : PlatformMaster
    {
        public override bool IsEditor { get; } = true;
        public override string Name { get; } = "Android";
        public override string PlatformName { get; } = "android";
        public override int GetNetSignal()
        {
            return 0;
        }
        
        public override void OpenAppSetting()
        {
            Log.I("OpenAppSetting Editor");
        }
        public override void SendMsgToNative(string msg)
        {
            Log.I("SendMsgToNative", ("Data", msg));
        }

        public override void RequestUserPermission(string permission)
        {
            Log.I("RequestUserPermission", ("Permission", permission));
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
            Log.I("InstallApp", ("AppPath", appPath));
        }
        public override void Vibrate()
        {
            Log.I("Vibrate Editor");
        }
        public override void QuitUnityPlayer()
        {
            EditorApplication.isPlaying = false;
            Log.I("Quit Editor");
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}