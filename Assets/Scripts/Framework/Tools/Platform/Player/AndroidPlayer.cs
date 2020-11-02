using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class AndroidPlayer : PlatformManager
    {
        public override bool IsEditor()
        {
            return false;
        }
        public override string Name()
        {
            return "Android";
        }
        private AndroidJavaObject JavaObject()
        {
            return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
}
