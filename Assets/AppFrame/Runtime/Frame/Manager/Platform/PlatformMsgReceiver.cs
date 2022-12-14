using System.Collections;
using System.Collections.Generic;
using AppFrame.Tools;
using UnityEngine;

namespace AppFrame.Manager
{
    public class PlatformMsgReceiver : SingletonMonoEvent<PlatformMsgReceiver>
    {
        public override void InitParent(Transform parent)
        {
            base.InitParent(parent);
        }
    }
}