using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// AssetBundle管理类 用户只需要调用这个脚本里的 API 即可
/// </summary>
public class AssetBundleManager : SingletonMono<AssetBundleManager>
{
    public void Awake()
    {
        // 先加载 Manifest 文件
        ManifestLoader.Instance.Load();
    }

    /// <summary>
    /// 场景名 和 场景里面所有的包的管理 的映射
    /// </summary>
    private Dictionary<string, AssetbundleSceneManager> nameSceneDic = new Dictionary<string, AssetbundleSceneManager>();

    /// <summary>
    /// 加载资源包
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="folderName"></param>
    /// <param name="lp"></param>
    public void LoadAssetBundle(string sceneName, string folderName, LoadProgress lp)
    {
        if (!nameSceneDic.ContainsKey(sceneName))
        {
            // 没有这个场景: 先创建这个场景,再读取一下配置文件
            CreateSceneBundle(sceneName);
        }

        AssetbundleSceneManager sm = nameSceneDic[sceneName];
        // 开始加载资源包
        sm.LoadAssetBundle(folderName, lp, LoadAssetBundleCallBack);
    }

    /// <summary>
    /// 创建一个场景对应的包
    /// </summary>
    /// <param name="sceneName"></param>
    private void CreateSceneBundle(string sceneName)
    {
        AssetbundleSceneManager sm = new AssetbundleSceneManager(sceneName);
        // 读取记录文件
        sm.ReadRecord(sceneName);

        nameSceneDic.Add(sceneName, sm);
    }

    /// <summary>
    /// 加载资源包的回调
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="bundleName"></param>
    private void LoadAssetBundleCallBack(string sceneName, string bundleName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            StartCoroutine(sm.Load(bundleName));
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
        }
    }

    /// <summary>
    /// 获取完整包名
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public string GetBundleName(string sceneName, string folderName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            return sm.GetBundleName(folderName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return null;
        }
    }

    /// <summary>
    /// 是否加载了这个文件夹
    /// </summary>
    /// <param name="folderName">文件夹名</param>
    /// <returns></returns>
    public bool IsLoading(string sceneName, string folderName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            return sm.IsLoading(folderName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return false;
        }
    }

    /// <summary>
    /// 是否加载完成这个包
    /// </summary>
    /// <param name="folderName">文件夹名</param>
    /// <returns></returns>
    public bool IsFinished(string sceneName, string folderName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            return sm.IsFinished(folderName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return false;
        }
    }

    #region 下层提供的 API
    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns>Obj类型的资源</returns>
    public Object LoadAsset(string sceneName, string folderName, string assetName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            return sm.LoadAsset(folderName, assetName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return null;
        }
    }

    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns>Obj类型的资源</returns>
    public T LoadAsset<T>(string sceneName, string folderName, string assetName) where T : UnityEngine.Object
    {
        return LoadAsset(sceneName, folderName, assetName) as T;
    }

    /// <summary>
    /// 获取包里的所有资源
    /// </summary>
    /// <returns></returns>
    public Object[] LoadAllAssets(string sceneName, string folderName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            return sm.LoadAllAssets(folderName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return null;
        }
    }

    /// <summary>
    /// 获取带有子物体的资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns>所有资源</returns>
    public Object[] LoadAssetWithSubAssets(string sceneName, string folderName, string assetName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            return sm.LoadAssetWithSubAssets(folderName, assetName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return null;
        }
    }

    /// <summary>
    /// 卸载一个包里的单个资源
    /// </summary>
    /// <param name="asset"></param>
    public void UnloadAsset(string sceneName, string folderName, string assetName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            sm.UnloadAsset(folderName, assetName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return;
        }
    }

    /// <summary>
    /// 卸载一个包里的所有资源
    /// </summary>
    /// <param name="bundleName"></param>
    public void UnLoadAllAssets(string sceneName, string folderName)
    {

        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            sm.UnLoadAllAssets(folderName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return;
        }
    }

    /// <summary>
    /// 卸载所有的资源
    /// </summary>
    public void UnLoadAll(string sceneName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            sm.UnLoadAll();
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return;
        }
    }

    /// <summary>
    /// 卸载单个 bundle 包
    /// </summary>
    public void Dispose(string sceneName, string folderName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            sm.Dispose(folderName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return;
        }
    }

    /// <summary>
    /// 卸载所有 bundle包
    /// </summary>
    public void DisposeAll(string sceneName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            sm.DisposeAll();
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return;
        }
    }

    /// <summary>
    /// 包和资源全部卸载
    /// </summary>
    public void DisposeAndUnLoadAll(string sceneName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            sm.DisposeAndUnLoadAll();
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return;
        }
    }

    /// <summary>
    /// 调试专用
    /// </summary>
    public void GetAllAssetNames(string sceneName, string folderName)
    {
        if (nameSceneDic.ContainsKey(sceneName))
        {
            AssetbundleSceneManager sm = nameSceneDic[sceneName];
            sm.GetAllAssetNames(folderName);
        }
        else
        {
            Debug.Log("Error:不存在该场景:" + sceneName);
            return;
        }
    }

    private void OnDestroy()
    {
        nameSceneDic.Clear();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }
    #endregion
}