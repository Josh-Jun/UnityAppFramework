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
            return "Windows";
        }
    }
}
