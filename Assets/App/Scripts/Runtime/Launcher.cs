using System;
using System.Linq;
using App.Runtime.Code;
using App.Runtime.Helper;
using App.Runtime.Hotfix;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace App.Runtime
{
    public class Launcher : MonoBehaviour
    {
        private HotfixView _hotfix = null;

        private void Awake()
        {
            _hotfix = transform.Find("Canvas").GetComponent<HotfixView>();
            Global.AppConfig = Resources.Load<AppConfig>("App/AppConfig");
            // YooAssets初始化
            YooAssets.Initialize();
            UniTask.Void(async () =>
            {
                // 创建默认包
                await Assets.UpdatePackage(AssetPackage.BuiltinPackage, _hotfix.SetDownloadProgress, true);
                CodeLoader.Start();
                // 加载AppScene
                await Assets.LoadSceneAsync(AssetPath.AppScene);
            });
        }
    }
}