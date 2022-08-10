using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AssetBundleManager : SingletonMono<AssetBundleManager>
{
    public Dictionary<string, Dictionary<string, string>> ABScenePairs { get; private set; } =
        new Dictionary<string, Dictionary<string, string>>();

    private Dictionary<string, AssetBundle> AssetBundlesCache = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// Manifest 文件
    /// </summary>
    private AssetBundleManifest mainfest;

    /// <summary>
    /// 全局存在的 AssetBundle
    /// </summary>
    private AssetBundle assetbundle;

    private string mainfestPath;

    public override void InitManager(Transform parent)
    {
        base.InitManager(parent);
        if (PlatformManager.Instance.IsEditor)
        {
            mainfestPath = PlatformManager.Instance.GetDataPath(PlatformManager.Instance.Name);
        }
        else
        {
            mainfestPath = $"file://{PlatformManager.Instance.GetDataPath(PlatformManager.Instance.Name)}";
        }
    }

    public void SetAbScenePairs(string sceneNane, Dictionary<string, string> folderPairs)
    {
        if (!ABScenePairs.ContainsKey(sceneNane))
        {
            ABScenePairs.Add(sceneNane, folderPairs);
        }
    }

    public AssetBundle LoadAssetBundle(string bundleName)
    {
        AssetBundle ab;
        //加载ab包，需一并加载其依赖包。
        if (assetbundle == null)
        {
            //根据各个平台下的基础路径和主包名加载主包
            assetbundle = AssetBundle.LoadFromFile($"{mainfestPath}/{PlatformManager.Instance.Name}");
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

        //加载目标包 -- 同理注意缓存问题
        if (AssetBundlesCache.ContainsKey(bundleName)) return AssetBundlesCache[bundleName];
        else
        {
            ab = AssetBundle.LoadFromFile($"{mainfestPath}/{bundleName}");
            AssetBundlesCache.Add(bundleName, ab);
            return ab;
        }
    }
    public AssetBundle LoadAssetBundle(string sceneName, string folderName)
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        AssetBundle ab;
        //加载ab包，需一并加载其依赖包。
        if (assetbundle == null)
        {
            //根据各个平台下的基础路径和主包名加载主包
            assetbundle = AssetBundle.LoadFromFile($"{mainfestPath}/{PlatformManager.Instance.Name}");
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

        //加载目标包 -- 同理注意缓存问题
        if (AssetBundlesCache.ContainsKey(bundleName)) return AssetBundlesCache[bundleName];
        else
        {
            ab = AssetBundle.LoadFromFile($"{mainfestPath}/{bundleName}");
            AssetBundlesCache.Add(bundleName, ab);
            return ab;
        }
    }

    public T LoadAsset<T>(string sceneName, string folderName, string assetsName) where T : UnityEngine.Object
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        AssetBundle ab = LoadAssetBundle(bundleName);
        return ab.LoadAsset<T>(assetsName);
    }

    public UnityEngine.Object LoadAsset(string sceneName, string folderName, string assetsName)
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        AssetBundle ab = LoadAssetBundle(bundleName);
        return ab.LoadAsset(assetsName);
    }

    public UnityEngine.Object LoadAsset(string sceneName, string folderName, string assetsName, System.Type type)
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        AssetBundle ab = LoadAssetBundle(bundleName);
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
        string bundleName = ABScenePairs[sceneName][folderName];
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