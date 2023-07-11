using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace AppFrame.Manager
{
    public class EditorPlayer : PlatformManager
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
            
        }

        public override void RequestUserPermission(string permission)
        {
            
        }

        public override string GetDataPath(string folder)
        {
            return $"{Application.persistentDataPath}/{folder}";
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
#if UNITY_EDITOR
            Log.I("Vibrate Editor");
#endif
        }
        public override void QuitUnityPlayer()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Log.I("Quit Editor");
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
