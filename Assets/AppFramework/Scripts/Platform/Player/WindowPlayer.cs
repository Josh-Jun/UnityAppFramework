using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class WindowPlayer : PlatformManager
    {
        public override bool IsEditor()
        {
            return false;
        }
        public override string Name()
        {
            return "Windows";
        }
        public override string GetDataPath(string folder)
        {
            return string.Format("{0}{1}", Application.dataPath.Replace("Assets", ""), folder);
        }
        public override void SavePhoto(string folder, string fileName)
        {
            Debug.Log("SavePhoto");
        }
        public override void QuitUnityPlayer(bool isStay = false)
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
