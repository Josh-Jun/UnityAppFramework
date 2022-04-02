using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 03
/// 管理 一个 bundle 包的依赖关系
/// </summary>
public class AssetBundleRelation
{
    /// <summary>
    /// bundle 包[02对象]{上层定义一个下层的对象,来传递}
    /// </summary>
    private AssetBundleLoader assetBundleLoader;

    public string BundleName { get; }

    public LoadProgress Lp { get; }

    public bool IsFinish { get; private set; }


    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="bundleName"></param>
    /// <param name="lp"></param>
    public AssetBundleRelation(string bundleName, LoadProgress lp)
    {
        this.BundleName = bundleName;
        this.Lp = lp;
        this.IsFinish = false;

        assetBundleLoader = new AssetBundleLoader(OnLoadComplete, lp, bundleName);
        dependenceBundleList = new List<string>();
        referenceBundleList = new List<string>();
    }

    /// <summary>
    /// 加载完成的回调(lc)
    /// </summary>
    /// <param name="bundleName">包名</param>
    private void OnLoadComplete(string bundleName)
    {
        this.IsFinish = true;
    }

    /// <summary>
    /// 加载资源包(供上层调用)
    /// </summary>
    /// <returns></returns>
    public void Load()
    {
        assetBundleLoader.Load();
    }

    #region 依赖关系
    /// <summary>
    /// 所有依赖的包名[在构造函数中new了]
    /// </summary>
    private List<string> dependenceBundleList;

    /// <summary>
    /// 添加依赖关系
    /// </summary>
    /// <param name="bundleName">包名</param>
    public void AddDependence(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
            return;
        else if (dependenceBundleList.Contains(bundleName))
            return;
        else
            dependenceBundleList.Add(bundleName);
    }

    /// <summary>
    /// 移除依赖关系
    /// </summary>
    /// <param name="bundleName"></param>
    public void RemoveDependence(string bundleName)
    {
        if (dependenceBundleList.Contains(bundleName))
            dependenceBundleList.Remove(bundleName);
    }

    /// <summary>
    /// 获取所有依赖关系[测试用]
    /// </summary>
    /// <returns></returns>
    public string[] GetAllDependence()
    {
        return dependenceBundleList.ToArray();
    }

    #endregion

    #region 被依赖关系
    /// <summary>
    /// 所有被依赖的包名[在构造函数中new了]
    /// </summary>
    private List<string> referenceBundleList;

    /// <summary>
    /// 添加被依赖关系
    /// </summary>
    /// <param name="bundleName">包名</param>
    public void AddReference(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
            return;
        else if (referenceBundleList.Contains(bundleName))
            return;
        else
            referenceBundleList.Add(bundleName);
    }

    /// <summary>
    /// 移除被依赖关系
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns>true: 包被释放掉了; false: 没释放</returns>
    public bool RemoveReference(string bundleName)
    {
        if (referenceBundleList.Contains(bundleName))
        {
            referenceBundleList.Remove(bundleName);

            // 移除一个包的时候要做一个判断
            // 检查还有没有被别的包所依赖

            // 有 : 就保留这个 bunelde 包 , 不能删除 ,返回false
            if (referenceBundleList.Count > 0)
                return false;
            else
            {
                // 无 : 释放掉这个AssetBundle ,返回true
                // Dispose(); 放到OnSceneAB的Dispose方法里面了,递归
                return true;
            }

        }
        else
            return false;

    }

    /// <summary>
    /// 获取所有被依赖关系[测试用]
    /// </summary>
    /// <returns></returns>
    public string[] GetAllReference()
    {
        return referenceBundleList.ToArray();
    }
    #endregion

    #region 下层 API
    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns>Obj类型的资源</returns>
    public Object LoadAsset(string assetName)
    {
        if (assetBundleLoader == null)
        {
            Debuger.LogError("当前assetBundleLoader为空!" + assetBundleLoader);
            return null;
        }

        return assetBundleLoader.LoadAsset(assetName);
    }

    /// <summary>
    /// 获取包里的所有资源
    /// </summary>
    /// <returns></returns>
    public Object[] LoadAllAssets()
    {
        if (assetBundleLoader == null)
        {
            Debuger.LogError("当前assetBundleLoader为空!" + assetBundleLoader);
            return null;
        }
        return assetBundleLoader.LoadAllAssets();
    }

    /// <summary>
    /// 获取带有子物体的资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns>所有资源</returns>
    public Object[] LoadAssetWithSubAssets(string assetName)
    {
        if (assetBundleLoader == null)
        {
            Debuger.LogError("当前assetBundleLoader为空!" + assetBundleLoader);
            return null;
        }

        return assetBundleLoader.LoadAssetWithSubAssets(assetName);
    }

    /// <summary>
    /// 卸载资源[Object类型]
    /// </summary>
    /// <param name="asset"></param>
    public void UnloadAsset(Object asset)
    {
        if (assetBundleLoader == null)
        {
            Debuger.LogError("当前assetBundleLoader为空!" + assetBundleLoader);
        }
        else
            assetBundleLoader.UnloadAsset(asset);
    }

    /// <summary>
    /// 释放资源包
    /// </summary>
    public void Dispose()
    {
        if (assetBundleLoader == null)
            return;

        assetBundleLoader.Dispose();
        // false : 只卸载包
        // true  : 卸载包和obj
        assetBundleLoader = null; //释放资源后, 把 assetBundleLoader 置空
    }

    /// <summary>
    /// 调试专用
    /// </summary>
    public void GetAllAssetNames()
    {
        assetBundleLoader.GetAllAssetNames();
    }
    #endregion
}