using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppFrame.Config;
using UnityEngine;

namespace Launcher
{
    public class AssetBundleManager : MonoBehaviour
    {
        private static AssetBundleManager _Instance = null;

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

        private string mainfestPath;

        public static string PlatformName;

        public static AssetBundleManager Instance
        {
            get
            {
                lock ("SingletonLock")
                {
                    if (_Instance == null)
                    {
                        GameObject go = new GameObject(typeof(AssetBundleManager).Name);
                        _Instance = go.AddComponent<AssetBundleManager>();
                    }
                    return _Instance;
                }
            }
        }

        public static bool IsEditor
        {
            get
            {
                return Application.platform == RuntimePlatform.WindowsEditor ||
                       Application.platform == RuntimePlatform.OSXEditor;
            }
        }

        public void InitManager()
        {
            var head = IsEditor ? "" : "file://";
            PlatformName = Launcher.AppConfig.TargetPackage == TargetPackage.Mobile ? "Android" : Launcher.AppConfig.TargetPackage.ToString();
            mainfestDataPath = $"{head}{Application.persistentDataPath}/AssetBundle/{PlatformName}/{Application.version}/{Launcher.AppConfig.ResVersion}/Hybrid";
            mainfestAssetsPath =$"{head}{Application.streamingAssetsPath}/AssetBundle/{PlatformName}/{Application.version}/{Launcher.AppConfig.ResVersion}/Hybrid";
            
            switch (Launcher.AppConfig.LoadAssetsMold)
            {
                case LoadAssetsMold.Local:
                    mainfestPath = mainfestAssetsPath;
                    break;
                case LoadAssetsMold.Remote:
                    mainfestPath = mainfestDataPath;
                    break;
            }
        }

        public void SetAbModulePairs(string moduleName, Dictionary<string, Folder> folderPairs)
        {
            if (!ABModulePairs.ContainsKey(moduleName))
            {
                ABModulePairs.Add(moduleName, folderPairs);
            }
        }

        public AssetBundle LoadAssetBundle(string bundleName, int mold)
        {
            AssetBundle ab;

            if (mold == 0)
            {
                //加载ab包，需一并加载其依赖包。
                if (assetbundle == null)
                {
                    //根据各个平台下的基础路径和主包名加载主包
                    assetbundle = AssetBundle.LoadFromFile($"{mainfestPath}/{PlatformName}");
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
                        ab = AssetBundle.LoadFromFile($"{mainfestPath}/{dependencies[i]}");
                        //注意添加进缓存 防止重复加载AB包
                        AssetBundlesCache.Add(dependencies[i], ab);
                    }
                }
            }

            //加载目标包 -- 同理注意缓存问题
            if (AssetBundlesCache.ContainsKey(bundleName)) return AssetBundlesCache[bundleName];
            else
            {
                ab = AssetBundle.LoadFromFile($"{mainfestPath}/{bundleName}");
                AssetBundlesCache.Add(bundleName, ab);
                return ab;
            }
        }

        public AssetBundle LoadAssetBundle(string moduleName, string folderName)
        {
            Folder folder = ABModulePairs[moduleName][folderName];
            return LoadAssetBundle(folder.BundleName, int.Parse(folder.Mold));
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