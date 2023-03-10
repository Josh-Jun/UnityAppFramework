using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppFrame.Config;
using AppFrame.Enum;
using AppFrame.Info;
using AppFrame.Tools;
using AppFrame.Network;
using UnityEngine;

namespace AppFrame.Manager
{
    public class AssetBundleManager : SingletonMono<AssetBundleManager>
    {
        public Dictionary<string, Dictionary<string, Folder>> ABModulePairs { get; private set; } =
            new Dictionary<string, Dictionary<string, Folder>>();

        private Dictionary<string, AssetBundle> AssetBundlesCache = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// Manifest 文件
        /// </summary>
        private AssetBundleManifest mainfest;

        /// <summary>
        /// 全局存在的 AssetBundle
        /// </summary>
        private AssetBundle assetbundle;

        private string mainfestDataPath;
        private string mainfestAssetsPath;

        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();

            var head = PlatformManager.Instance.IsEditor ? "" : "file://";
            mainfestDataPath = $"{head}{PlatformManager.Instance.GetDataPath(PlatformManager.Instance.Name)}";
            mainfestAssetsPath =$"{head}{PlatformManager.Instance.GetAssetsPath($"AssetBundle/{PlatformManager.Instance.Name}")}";
        }

        public void InitLocalAssetBundleConfig(Action callbcak = null)
        {
            UnityWebRequester requester = NetcomManager.Uwr;
            requester.Get($"{mainfestAssetsPath}/AssetBundleConfig.json", (string data) =>
            {
                AssetBundleConfig abc = JsonUtility.FromJson<AssetBundleConfig>(data);
                foreach (var module in abc.Modules)
                {
                    Dictionary<string, Folder> folderPairs = new Dictionary<string, Folder>();
                    foreach (var folder in module.Folders)
                    {
                        if (!folderPairs.ContainsKey(folder.FolderName))
                        {
                            folderPairs.Add(folder.FolderName, folder);
                        }
                    }

                    SetAbModulePairs(module.ModuleName, folderPairs);
                }

                callbcak?.Invoke();
            });
        }

        public void SetAbModulePairs(string moduleName, Dictionary<string, Folder> folderPairs)
        {
            if (!ABModulePairs.ContainsKey(moduleName))
            {
                ABModulePairs.Add(moduleName, folderPairs);
            }
        }

        public AssetBundle LoadAssetBundle(string bundleName, bool isLocalAsset)
        {
            string mainPath = isLocalAsset ? mainfestAssetsPath : mainfestDataPath;
            AssetBundle ab;

            if (AppInfo.AppConfig.ABPipeline == ABPipeline.Default)
            {
                //加载ab包，需一并加载其依赖包。
                if (assetbundle == null)
                {
                    //根据各个平台下的基础路径和主包名加载主包
                    assetbundle = AssetBundle.LoadFromFile($"{mainPath}/{PlatformManager.Instance.Name}");
                    //获取主包下的AssetBundleManifest资源文件（存有依赖信息）
                    mainfest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                }

                //根据manifest获取所有依赖包的名称 固定API
                string[] dependencies = GetDependence(bundleName);
                //循环加载所有依赖包
                for (int i = 0; i < dependencies.Length; i++)
                {
                    //如果不在缓存则加入
                    if (!AssetBundlesCache.ContainsKey(dependencies[i]))
                    {
                        //根据依赖包名称进行加载
                        ab = AssetBundle.LoadFromFile($"{mainPath}/{dependencies[i]}");
                        //注意添加进缓存 防止重复加载AB包
                        AssetBundlesCache.Add(dependencies[i], ab);
                    }
                }
            }

            //加载目标包 -- 同理注意缓存问题
            if (AssetBundlesCache.ContainsKey(bundleName)) return AssetBundlesCache[bundleName];
            else
            {
                ab = AssetBundle.LoadFromFile($"{mainPath}/{bundleName}");
                AssetBundlesCache.Add(bundleName, ab);
                return ab;
            }
        }

        public AssetBundle LoadAssetBundle(string moduleName, string folderName)
        {
            Folder folder = ABModulePairs[moduleName][folderName];
            return LoadAssetBundle(folder.BundleName, folder.Mold == "1");
        }

        public T LoadAsset<T>(string moduleName, string folderName, string assetsName) where T : UnityEngine.Object
        {
            AssetBundle ab = LoadAssetBundle(moduleName, folderName);
            return ab.LoadAsset<T>(assetsName);
        }

        public UnityEngine.Object LoadAsset(string moduleName, string folderName, string assetsName)
        {
            AssetBundle ab = LoadAssetBundle(moduleName, folderName);
            return ab.LoadAsset(assetsName);
        }

        public UnityEngine.Object LoadAsset(string moduleName, string folderName, string assetsName, System.Type type)
        {
            AssetBundle ab = LoadAssetBundle(moduleName, folderName);
            return ab.LoadAsset(assetsName, type);
        }

        /// <summary>
        /// 获取一个 bundle 包的所有依赖关系[最重要]
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        private string[] GetDependence(string bundleName)
        {
            return mainfest.GetAllDependencies(bundleName);
        }

        public void UnloadCacheAssetBundles()
        {
            foreach (var pair in AssetBundlesCache)
            {
                if (pair.Value != null)
                {
                    pair.Value.Unload(false);
                }
            }

            AssetBundlesCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 单个包卸载
        /// </summary>
        public void UnLoad(string sceneName, string folderName)
        {
            string bundleName = ABModulePairs[sceneName][folderName].BundleName;
            if (AssetBundlesCache.ContainsKey(bundleName))
            {
                AssetBundlesCache[bundleName].Unload(false);
                //注意缓存需一并移除
                AssetBundlesCache.Remove(bundleName);
            }
        }

        /// <summary>
        /// 所有包卸载
        /// </summary>
        public void UnLoadAll()
        {
            AssetBundle.UnloadAllAssetBundles(false);
            //注意清空缓存
            AssetBundlesCache.Clear();
            assetbundle = null;
            mainfest = null;
        }
    }
}