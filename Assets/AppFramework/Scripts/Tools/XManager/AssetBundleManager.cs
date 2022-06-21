using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleManager : SingletonMono<AssetBundleManager>
{
    public Dictionary<string, Dictionary<string, string>> ABScenePairs { get; private set; } = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, AssetBundle> AssetBundlesCache = new Dictionary<string, AssetBundle>();
    private List<string> AllABPath = new List<string>();
    /// <summary>
    /// Manifest 文件
    /// </summary>
    private AssetBundleManifest manifest;
    
    /// <summary>
    /// 全局存在的 AssetBundle
    /// </summary>
    private AssetBundle assetbundle;

    private string mainfestPath;
    
    private void Awake()
    {
        mainfestPath = PlatformManager.Instance.GetDataPath(PlatformManager.Instance.Name);
    }

    public void InitManager()
    {
        transform.SetParent(App.app.transform);
    }

    public void SetAbScenePairs(string sceneNane, Dictionary<string, string> folderPairs)
    {
        if (!ABScenePairs.ContainsKey(sceneNane))
        {
            ABScenePairs.Add(sceneNane, folderPairs);
        }
    }
    
    public void LoadAssetBundle(Action<string, float> loadProgress = null)
    {
        if (assetbundle == null)
        {
            UnityWebRequester requester = new UnityWebRequester(App.app);
            requester.GetAssetBundle($"{mainfestPath}/{PlatformManager.Instance.Name}", (AssetBundle ab) =>
            {
                assetbundle = ab;
                manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                GetAssetBundlePath(loadProgress);
                requester.Destory();
            });
            return;
        }
        GetAssetBundlePath(loadProgress);
    }
    public void GetAssetBundlePath(Action<string, float> loadProgress = null)
    {
        foreach (var scene in ABScenePairs)
        {
            foreach (var folder in scene.Value)
            {
                string[] dependencies = GetDependence(folder.Value);
                //循环加载所有依赖包
                for (int i = 0; i < dependencies.Length; i++)
                {
                    int index = i;
                    //如果不在缓存则加入
                    if (!AllABPath.Contains(dependencies[index]))
                    {
                        AllABPath.Add(dependencies[index]);
                    }
                }
                if (!AllABPath.Contains(folder.Value))
                {
                    AllABPath.Add(folder.Value);
                }
            }
        }
        StartCoroutine(Load(loadProgress));
    }

    private IEnumerator Load(Action<string, float> loadProgress = null)
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < AllABPath.Count; i++)
        {
            int index = i;
            //根据依赖包名称进行加载
            UnityWebRequester requester = new UnityWebRequester(App.app);
            requester.GetAssetBundle($"{mainfestPath}/{AllABPath[index]}", (AssetBundle ab) =>
            {
                if (!AssetBundlesCache.ContainsKey(AllABPath[index]))
                {
                    //注意添加进缓存 防止重复加载AB包
                    AssetBundlesCache.Add(AllABPath[index], ab);
                }
            });
            while (!requester.IsDown)
            {
                float progress = requester.DownloadedProgress;
                loadProgress?.Invoke(AllABPath[index], progress);
            }
            yield return new WaitForEndOfFrame();
            loadProgress?.Invoke(AllABPath[index], 1);
        }
    }
    #region
    public T LoadAsset<T>(string sceneName, string folderName, string assetsName) where T : UnityEngine.Object
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        AssetBundle ab = AssetBundlesCache[bundleName];
        return ab.LoadAsset<T>(assetsName);
    }
    public UnityEngine.Object LoadAsset(string sceneName, string folderName, string assetName)
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        AssetBundle ab = AssetBundlesCache[bundleName];
        return ab.LoadAsset(assetName);
    }
    public UnityEngine.Object LoadAsset(string sceneName, string folderName, string assetName,System.Type type)
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        AssetBundle ab = AssetBundlesCache[bundleName];
        return ab.LoadAsset(assetName, type);
    }
    #endregion
    /// <summary>
    /// 获取一个 bundle 包的所有依赖关系[最重要]
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private string[] GetDependence(string bundleName)
    {
        return manifest.GetAllDependencies(bundleName);
    }
    /// <summary>
    /// 单个包卸载
    /// </summary>
    public void UnLoad(string sceneName, string folderName)
    {
        string bundleName = ABScenePairs[sceneName][folderName];
        if(AssetBundlesCache.ContainsKey(bundleName))
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
        manifest = null;
    }
}
