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
        
        private void HotfixReadyCompletedEvent()
        {
            // 获取云控数据
            CloudCtrlRequester.Post();
        }
    
        private async UniTask HotfixBeforeEvent()
        {
            if (RunUDPClient)
            {
                // UDP获取服务器TCP连接数据
                Global.ServerData = await UDPClient.GetServerData();
            }
            await UniTask.CompletedTask;
        }
    
        private async UniTask HotfixAfterEvent()
        {
            CodeLoader.Start();
            await UniTask.CompletedTask;
        }
    }
}
