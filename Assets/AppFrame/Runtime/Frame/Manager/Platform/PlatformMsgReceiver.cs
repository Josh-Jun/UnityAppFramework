using System.Collections;
using System.Collections.Generic;
using AppFrame.Tools;
using UnityEngine;

namespace AppFrame.Manager
{
    public class PlatformMsgReceiver : SingletonMonoEvent<PlatformMsgReceiver>
    {
        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
        }

        public void Init()
        {
            
        }

        public void AndroidPermissionCallbacks(string permission, int code)
        {
            
        }
        public void iOSPermissionCallbacks(string msg)
        {
            
        }
    }
}