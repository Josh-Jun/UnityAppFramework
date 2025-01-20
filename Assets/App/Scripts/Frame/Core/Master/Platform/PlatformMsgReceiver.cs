using App.Core.Tools;

namespace App.Core.Master
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