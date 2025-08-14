using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    [System.Serializable]
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
            #if UNITY_IOS && !UNITY_EDITOR
            AddEventMsg<string>(iOSPermissions.Camera, code => { PermissionCallbacks(iOSPermissions.Camera, int.Parse(code)); });
            AddEventMsg<string>(iOSPermissions.Photo, code => { PermissionCallbacks(iOSPermissions.Photo, int.Parse(code)); });
            AddEventMsg<string>(iOSPermissions.Microphone, code => { PermissionCallbacks(iOSPermissions.Microphone, int.Parse(code)); });
            AddEventMsg<string>(iOSPermissions.Networking, code => { PermissionCallbacks(iOSPermissions.Networking, int.Parse(code)); });
            #endif
        }
        public void PermissionCallbacks(string permission, int code)
        {
            if (HasEvent(permission))
            {
                SendEventMsg(permission, code);
            }
            else
            {
                Log.W($"PermissionCallbacks: not found permission event: {permission}, code: {code}");
            }
        }

        public void ReceiveNativeMsg(string msg)
        {
            var data = JsonUtility.FromJson<NativeData>(msg);
            if(HasEvent(data.Name)){
                SendEventMsg(data.Name, data.Data);
            }
            else{
                Log.W($"PlatformMsgReceiver[SendEventMsg]: {data.Name} not found");
            }
        }
    }
}