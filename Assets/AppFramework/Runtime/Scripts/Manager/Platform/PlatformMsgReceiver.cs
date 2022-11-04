using System.Collections;
using System.Collections.Generic;
using AppFramework.Tools;
using UnityEngine;

namespace AppFramework.Manager
{
    public class PlatformMsgReceiver : SingletonMonoEvent<PlatformMsgReceiver>
    {
        public override void InitParent(Transform parent)
        {
            base.InitParent(parent);
        }
    }
}