using App.Runtime.Helper;
using App.Runtime.Hotfix;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace App.Runtime
{
    public partial class Launcher : MonoBehaviour
    {
        private const string APP_CONFIG_PATH = "AppConfig";

        private void Awake()
        {
            // 加载应用配置文件
            Global.AppConfig = Resources.Load<AppConfig>(APP_CONFIG_PATH);
            // 加载配置文件之后，热更之前事件
            HotfixReadyCompletedEvent();
            // 弹出隐私UI界面，启动脚本热更
            HotfixView.Instance.Startup(() =>
            {
                // YooAssets初始化
                YooAssets.Initialize();
                UniTask.Void(async () =>
                {
                    // 热更之前事件
                    await HotfixBeforeEvent();
                    // 创建默认包
                    await Assets.UpdatePackage(AssetPackage.BuiltinPackage, HotfixView.Instance.SetDownloadProgress, true);
                    // 热更之后事件
                    await HotfixAfterEvent();
                    // 加载AppScene
                    await Assets.LoadSceneAsync(AssetPath.AppScene);
                });
            });
        }
    }
}