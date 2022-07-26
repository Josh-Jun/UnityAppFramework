using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Platform
{
    public class EditorPlayer : PlatformManager
    {
        public override bool IsEditor { get; } = true;
        public override string Name { get; } = "Android";
        public override string PlatformName { get; } = "android";
        public override string GetDataPath(string folder)
        {
            return $"{Application.persistentDataPath}/{folder}";
        }
        public override void SavePhoto(string imagePath)
        {
            Debug.Log("SavePhoto");
        }
        public override void InstallApp(string appPath)
        {
            
        }
        public override void QuitUnityPlayer()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Debug.Log("Quit Editor");
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
