using System.Collections.Generic;
using AppFrame.Config;
using UnityEngine;

namespace Launcher
{
    public class HybridABManager : MonoBehaviour
    {
        private static HybridABManager _Instance = null;

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

        public static HybridABManager Instance
        {
            get
            {
                lock ("SingletonLock")
                {
                    if (_Instance == null)
                    {
                        GameObject go = new GameObject(typeof(HybridABManager).Name);
                        _Instance = go.AddComponent<HybridABManager>();
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

        public static string PlatformName
        {
            get
            {
                string platform = "";
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsPlayer:
                        platform = "StandaloneWindows";
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        platform = "iOS";
                        break;
                    case RuntimePlatform.Android:
                        platform = "Android";
                        break;
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.OSXEditor:
                        platform = "Android";
                        break;
                    default:
                        break;
                }
                return platform;
            }
        }

        public void InitManager()
        {
            // OutputPath > 项目目录/AssetBundle/{buildTarget}/{Application.version}/{AppConfig.ResVersion}/{mold}
            var head = IsEditor ? "" : "file://";
            mainfestDataPath = $"{head}{Application.persistentDataPath}/AssetBundle/{PlatformName}/{Application.version}/{Global.AppConfig.ResVersion}/Hybrid";
            mainfestAssetsPath =$"{head}{Application.streamingAssetsPath}/AssetBundle/{PlatformName}/{Application.version}/{Global.AppConfig.ResVersion}/Hybrid";
            
            switch (Global.AppConfig.LoadAssetsMold)
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

            if (Global.AppConfig.ABPipeline == ABPipeline.Default)
            {
                assetbundle = AssetBundle.LoadFromFile($"{mainfestPath}/{PlatformName}");
                mainfest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
        }

        public AssetBundle LoadAssetBundle(string bundleName, string[] Dependencies = null)
        {
            AssetBundle ab;
            
            if (Dependencies != null)
            {
                for (int i = 0; i < Dependencies.Length; i++)
                {
                    //如果不在缓存则加入
                    if (!AssetBundlesCache.ContainsKey(Dependencies[i]))
                    {
                        //根据依赖包名称进行加载
                        ab = AssetBundle.LoadFromFile($"{mainfestPath}/{Dependencies[i]}");
                        //注意添加进缓存 防止重复加载AB包
                        AssetBundlesCache.Add(Dependencies[i], ab);
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
            return LoadAssetBundle(folder.BundleName, folder.Dependencies);
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
        /// <param name="moduleName"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        private string[] GetDependence(string moduleName, string folderName)
        {
            Folder folder = ABModulePairs[moduleName][folderName];
            return folder.Dependencies;
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
        public void UnLoad(string moduleName, string folderName)
        {
            string bundleName = ABModulePairs[moduleName][folderName].BundleName;
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
        }
    }
}