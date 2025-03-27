using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    public class NativeData
    {
        public string Name;
        public string Data;
    }
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

        public void ReceiveNativeMsg(string msg)
        {
            var data = JsonUtility.FromJson<NativeData>(msg);
            SendEventMsg(data.Name, data.Data);
        }
    }
}