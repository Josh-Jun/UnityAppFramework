/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年3月6 10:41
 * function    : 
 * ===============================================
 * */
using App.Runtime.CloudCtrl;
using App.Runtime.Code;
using App.Runtime.UDP;
using Cysharp.Threading.Tasks;

namespace App.Runtime
{
    public partial class Launcher
    {
        public bool RunUDPClient = false;
        
        private void LoadAppConfigCompletedEvent()
        {
            // 获取云控数据
            CloudCtrlRequester.Post(null);
        }
    
        private void HotfixBeforeEvent()
        {
            UniTask.Void(async () =>
            {
                if (RunUDPClient)
                {
                    CanMoveNext = false;
                    // UDP获取服务器TCP连接数据
                    Global.ServerData = await UDPClient.GetServerData();
                    CanMoveNext = true;
                }
            });
            
        }
    
        private void HotfixAfterEvent()
        {
            CodeLoader.Start();
        }
    }
}
