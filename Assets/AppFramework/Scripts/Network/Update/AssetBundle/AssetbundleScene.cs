using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 05
/// 管理一个场景里面所有的 bundle 包
/// </summary>
public class AssetbundleScene
{
    /// <summary>
    /// 包名 和对应的包的映射 (key : bundleName, value : assetbundle)
    /// </summary>
    private Dictionary<string, AssetBundleRelation> nameBundleDic;

    /// <summary>
    /// 包名 和 对应 包的缓存 的映射 (key : bundleName, value : 包里的Object)
    /// </summary>
    private Dictionary<string, AssetCaching> nameCacheDic;

    /// <summary>
    /// 当前场景的名字(需要上层传递)
    /// </summary>
    private string sceneName;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sceneName"></param>
    public AssetbundleScene(string sceneName)
    {
        this.sceneName = sceneName;
        nameBundleDic = new Dictionary<string, AssetBundleRelation>();
        nameCacheDic = new Dictionary<string, AssetCaching>();
    }

    /// <summary>
    /// 是否加载了这个包
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public bool IsLoading(string bundleName)
    {
        return nameBundleDic.ContainsKey(bundleName);
    }

    /// <summary>
    /// 是否加载完成这个包
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public bool IsFinished(string bundleName)
    {
        if (IsLoading(bundleName)) // 如果已经加载了这个包
        {
            return nameBundleDic[bundleName].IsFinish;
        }

        return false;
    }

    #region 加载包
    /// <summary>
    /// 加载资源包(给上层调用)
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="lp"></param>
    /// <param name="labcb"></param>
    public void LoadAssetBundle(string bundleName, LoadProgress lp, LoadAssetBundleCallback labcb)
    {
        if (nameBundleDic.ContainsKey(bundleName))
        {
            Debug.LogError("此包已经加载了!" + bundleName);
            return;
        }
        else // 没有加载的话 , 就 new 一个
        {
            AssetBundleRelation assetBundleRelation = new AssetBundleRelation(bundleName, lp);
            nameBundleDic.Add(bundleName, assetBundleRelation);

            // 开始加载
            //StartCoroutine("Load", bundleName); 不继承monobehavior,不能用协程
            labcb(sceneName, bundleName); // 让上层去加载
        }

    }

    /// <summary>
    /// 加载包[没有继承MOno,没法走协程,只能通过上面的方法加载资源包]
    /// </summary>\
    /// 
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public IEnumerator Load(string bundleName)
    {
        while (!ManifestLoader.Instance.IsFinish)
            yield return null;

        AssetBundleRelation assetBundleRelation = nameBundleDic[bundleName];
        // 先获取这个包所有的依赖关系
        string[] dependenceBundles = ManifestLoader.Instance.GetDependence(bundleName);

        // 添加这个包的所有依赖关系
        foreach (string item in dependenceBundles)
        {
            assetBundleRelation.AddDependence(item);

            // 加载这个包的所有依赖关系
            yield return LoadDependence(item, bundleName, assetBundleRelation.Lp);
        }

        // 开始加载这个包
        yield return assetBundleRelation.Load();
    }

    /// <summary>
    /// 加载依赖的包
    /// </summary>
    /// <param name="bundleName">包名</param>
    /// <param name="referenceBundleName">被依赖的包名</param>
    /// <param name="lp">进度回调</param>
    /// <returns></returns>
    private IEnumerator LoadDependence(string bundleName, string referenceBundleName, LoadProgress lp)
    {
        if (nameBundleDic.ContainsKey(bundleName))
        {
            // 已经加载过,就直接添加他的 "被" 依赖关系
            AssetBundleRelation assetBundleRelation = nameBundleDic[bundleName];
            // 添加这个包的 "被" 依赖关系
            assetBundleRelation.AddReference(referenceBundleName);
        }
        else
        {
            // 没加载过 ,就创建一个新的
            AssetBundleRelation assetBundleRelation = new AssetBundleRelation(bundleName, lp);
            // 添加这个包的 "被" 依赖关系
            assetBundleRelation.AddReference(referenceBundleName);
            // 保存到字典里
            nameBundleDic.Add(bundleName, assetBundleRelation);

            // 开始加载这个依赖的包
            yield return Load(bundleName);
        }
    }
    #endregion

    #region 加载
    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns>Obj类型的资源</returns>
    public Object LoadAsset(string bundleName, string assetName)
    {
        // 第一次加载 : 层层加载,并存到字典里;
        // 第二次加载 : 到字典里找,有就用,没有就再层层加载;

        // 1.先判断缓存没缓存?
        if (nameCacheDic.ContainsKey(bundleName))
        {
            Object[] assets = nameCacheDic[bundleName].GetAsset(assetName);
            if (assets != null)
                return assets[0]; //获取单个嘛,肯定就一个

        }

        // 2.当前包有没有被加载
        if (!nameBundleDic.ContainsKey(bundleName))
        {
            Debug.LogError("当前" + bundleName + "包没有加载,无法获取资源!");
            return null;
        }

        // 3.当前包已经被加载了: 层层加载,存到缓存里

        Object asset = nameBundleDic[bundleName].LoadAsset(assetName);
        AssetObject tempAsset = new AssetObject(asset);
        // 有这个缓存层,里面也有资源 , 但是这次获取资源的名字,是以前没缓存过的
        if (nameCacheDic.ContainsKey(bundleName))
        {
            // 直接加进去
            nameCacheDic[bundleName].AddAsset(assetName, tempAsset);
        }
        else
        {
            // 当前包已经被加载了,但是 是第一次获取这个包里的资源
            // 创建一个新的缓存层
            AssetCaching caching = new AssetCaching();
            caching.AddAsset(assetName, tempAsset);

            // 保存,下次使用
            nameCacheDic.Add(bundleName, caching);
        }
        return asset;
    }

    /// <summary>
    /// 获取包里的所有资源
    /// </summary>
    /// <returns></returns>
    public Object[] LoadAllAssets(string bundleName)
    {
        if (nameBundleDic.ContainsKey(bundleName))
        {
            return nameBundleDic[bundleName].LoadAllAssets();
        }
        return null;
    }

    /// <summary>
    /// 获取带有子物体的资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns>所有资源</returns>
    public Object[] LoadAssetWithSubAssets(string bundleName, string assetName)
    {

        // 1.先判断缓存没缓存?
        if (nameCacheDic.ContainsKey(bundleName))
        {
            Object[] assetsArr = nameCacheDic[bundleName].GetAsset(assetName);
            if (assetsArr != null)
                return assetsArr;

        }

        // 2.当前包有没有被加载
        if (!nameBundleDic.ContainsKey(bundleName))
        {
            Debug.LogError("当前" + bundleName + "包没有加载,无法获取资源!");
            return null;
        }

        // 3.当前包已经被加载了,

        Object[] asset = nameBundleDic[bundleName].LoadAssetWithSubAssets(assetName);
        AssetObject tempAsset = new AssetObject(asset);
        // 有这个缓存层,里面也有资源 , 但是这次获取资源的名字,是以前没缓存过的
        if (nameCacheDic.ContainsKey(bundleName))
        {
            // 直接加进去
            nameCacheDic[bundleName].AddAsset(assetName, tempAsset);
        }
        else
        {
            // 当前包已经被加载了,但是 是第一次获取这个包里的资源
            // 创建一个新的缓存层
            AssetCaching caching = new AssetCaching();
            caching.AddAsset(assetName, tempAsset);

            // 保存,下次使用
            nameCacheDic.Add(bundleName, caching);
        }
        return asset;
    }
    #endregion

    #region 卸载
    /// <summary>
    /// 卸载一个包里的单个资源
    /// </summary>
    /// <param name="asset"></param>
    public void UnloadAsset(string bundleName, string assetName)
    {
        if (!nameCacheDic.ContainsKey(bundleName))
        {
            Debug.LogError("当前" + bundleName + "包没有缓存资源,无法卸载资源!");
        }
        else
        {
            nameCacheDic[bundleName].UnLoadAsset(assetName);
            Resources.UnloadUnusedAssets();
        }

    }

    /// <summary>
    /// 卸载一个包里的所有资源
    /// </summary>
    /// <param name="bundleName"></param>
    public void UnLoadAllAssets(string bundleName)
    {
        if (!nameCacheDic.ContainsKey(bundleName))
        {
            Debug.LogError("当前" + bundleName + "包没有缓存资源,无法卸载资源!");
        }
        else
        {
            nameCacheDic[bundleName].UnLoadAllAsset();
            nameCacheDic.Remove(bundleName);
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// 卸载所有的资源
    /// </summary>
    public void UnLoadAll()
    {
        foreach (string item in nameCacheDic.Keys)
        {
            UnLoadAllAssets(item);
        }
        nameCacheDic.Clear();
    }

    /// <summary>
    /// 卸载一个 bundle 包
    /// </summary>
    public void Dispose(string bundleName)
    {
        if (!nameBundleDic.ContainsKey(bundleName))
        {
            Debug.LogError("当前" + bundleName + "包没有被加载,无法获取资源!");
            return;
        }

        // 先得到当前的包
        AssetBundleRelation assetBundleRelation = nameBundleDic[bundleName];

        // 获取当前包所有的依赖关系
        string[] allDependences = assetBundleRelation.GetAllDependence();

        foreach (string item in allDependences)
        {
            // 移除 依赖的包里面的被依赖关系
            AssetBundleRelation tempassetBundleRelation = nameBundleDic[item];
            bool isDispose = tempassetBundleRelation.RemoveReference(bundleName);
            if (isDispose) // 没有其他依赖项了(可以释放了)
            {
                // 递归
                Dispose(tempassetBundleRelation.BundleName);
            }

        }

        // 开始卸载当前包 
        if (assetBundleRelation.GetAllReference().Length <= 0) //没有被依赖项了
        {
            nameBundleDic[bundleName].Dispose();
            nameBundleDic.Remove(bundleName);
        }
    }

    /// <summary>
    /// 卸载所有 bundle 包
    /// </summary>
    public void DisposeAll()
    {
        foreach (string item in nameBundleDic.Keys)
        {
            Dispose(item);
        }
        nameBundleDic.Clear();
    }

    /// <summary>
    /// 卸载所有的bundle包和资源
    /// </summary>
    public void DisposeAndUnLoadAll()
    {
        UnLoadAll();
        DisposeAll();
    }

    #endregion

    /// <summary>
    /// 调试专用
    /// </summary>
    public void GetAllAssetNames(string bundleName)
    {
        nameBundleDic[bundleName].GetAllAssetNames();
    }
}