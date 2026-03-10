using System.Threading;
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
        private CancellationTokenSource cancel = new();
        private bool CanMoveNext = true;

        private void Awake()
        {
            // 加载应用配置文件
            Global.AppConfig = Resources.Load<AppConfig>(APP_CONFIG_PATH);
            // 加载配置文件之后，热更之前事件
            SendMessage("LoadAppConfigCompletedEvent");
            // 弹出隐私UI界面，启动脚本热更
            HotfixView.Instance.Startup(() =>
            {
                // YooAssets初始化
                YooAssets.Initialize();
                UniTask.Void(async () =>
                {
                    SendMessage("HotfixBeforeEvent");
                    cancel = new CancellationTokenSource();
                    // 热更之前事件
                    await UniTask.WaitUntil(() => CanMoveNext);
                    // 创建默认包
                    await Assets.UpdatePackage(AssetPackage.BuiltinPackage, HotfixView.Instance.SetDownloadProgress, true);
                    // 热更之后事件
                    SendMessage("HotfixAfterEvent");
                    // 加载AppScene
                    await Assets.LoadSceneAsync(AssetPath.AppScene);
                });
            });
        }
    }
}