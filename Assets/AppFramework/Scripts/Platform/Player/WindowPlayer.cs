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
        public override void QuitUnityPlayer()
        {
            Debug.Log("Quit");
        }
        public override string GetAppData(string key)
        {
            return "";
        }
    }
}
