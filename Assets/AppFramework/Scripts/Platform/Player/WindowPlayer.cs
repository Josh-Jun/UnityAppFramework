using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class WindowPlayer : PlatformManager
    {
        public override bool IsEditor { get; } = false;
        public override string Name { get; } = "Windows";
        public override string PlatformName { get; } = "window";
        public override string GetDataPath(string folder)
        {
            return $"file://{Application.dataPath.Replace("Assets", "")}{folder}";
        }
        public override void SavePhoto(string folder, string fileName)
        {
            Debug.Log("SavePhoto");
        }
        public override void QuitUnityPlayer()
        {
            Debug.Log("Quit Window");
            Application.Quit();
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
