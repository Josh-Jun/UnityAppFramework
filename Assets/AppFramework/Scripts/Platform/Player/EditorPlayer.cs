using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class EditorPlayer : PlatformManager
    {
        public override bool IsEditor()
        {
            return true;
        }
        public override string Name()
        {
            return "Android";
        }
        public override string GetPath(string folder)
        {
            return string.Format("{0}/{1}/", Application.persistentDataPath, folder);
        }
        public override void SavePhoto(string fileName)
        {
            Debug.Log("SavePhoto");
        }
        public override void QuitUnityPlayer()
        {
            Debug.Log("Quit Editor");
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
