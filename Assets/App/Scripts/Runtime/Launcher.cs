using App.Runtime.CloudCtrl;
using App.Runtime.Code;
using App.Runtime.Helper;
using App.Runtime.Hotfix;
using App.Runtime.UDP;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace App.Runtime
{
    public class Launcher : MonoBehaviour
    {
        private const string APP_CONFIG_PATH = "AppConfig";
        private HotfixView _hotfix = null;

        public bool RunUDPClient = false;
        private void Awake()
        {
            // 获取热更UI界面
            _hotfix = transform.Find("Canvas").GetComponent<HotfixView>();
            // 加载应用配置文件
            Global.AppConfig = Resources.Load<AppConfig>(APP_CONFIG_PATH);
            // 获取云控数据
            CloudCtrlRequester.Post();
            // 弹出隐私UI界面，启动脚本热更
            _hotfix.Startup(() =>
            {
                UniTask.Void(async () =>
                {
                    if (RunUDPClient)
                    {
                        // UDP获取服务器TCP连接数据
                        Global.ServerData = await UDPClient.GetServerData();
                    }
                    // YooAssets初始化
                    YooAssets.Initialize();
                    // 创建默认包
                    await Assets.UpdatePackage(AssetPackage.BuiltinPackage, _hotfix.SetDownloadProgress, true);
                    CodeLoader.Start();
                    // 加载AppScene
                    await Assets.LoadSceneAsync(AssetPath.AppScene);
                });
            });
        }
    }
}