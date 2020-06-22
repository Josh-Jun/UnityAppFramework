using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
    public class IPhonePlayer : PlatformManager
    {
        public override bool IsEditor()
        {
            return false;
        }
        public override string Name()
        {
            return "iOS";
        }
        public override string CachePath()
        {
            return string.Format(@"{0}/", Application.persistentDataPath);
        }
    }
}
