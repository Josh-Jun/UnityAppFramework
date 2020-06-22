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
        public override string CachePath()
        {
            return string.Format(@"{0}/", Application.persistentDataPath);
        }
    }
}
