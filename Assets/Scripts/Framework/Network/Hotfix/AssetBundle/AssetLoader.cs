using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 01
/// 加载 Asset (加载包里的资源)
/// 继承 IDisposable 释放资源用
/// </summary>
public class AssetLoader : System.IDisposable
{
    /// <summary>
    /// 当前资源包
    /// </summary>
    private AssetBundle assetbundle;

    /// <summary>
    /// 构造 (上层创建这个类的时候,给assetBundle赋值)
    /// </summary>
    /// <param name="ab"></param>
    public AssetLoader(AssetBundle ab)
    {
        this.assetbundle = ab;
    }

    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns> Obj 类型的资源 </returns>
    public Object LoadAsset(string assetName)
    {
        if (assetbundle == null)
        {
            Debug.LogError("当前资源包为空,无法获取" + assetName + "资源");
            return null;
        }
        else if (!assetbundle.Contains(assetName))
        {
            Debug.LogError("当前资源不包含" + assetName + "资源");
            return null;
        }
        else
            return assetbundle.LoadAsset(assetName);
    }

    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns> Obj 类型的资源 </returns>
    public T LoadAsset<T>(string assetName) where T : class
    {
        if (assetbundle == null)
        {
            Debug.LogError("当前资源包为空,无法获取" + assetName + "资源");
            return null;
        }
        else if (!assetbundle.Contains(assetName))
        {
            Debug.LogError("当前资源不包含" + assetName + "资源");
            return null;
        }
        else
            return assetbundle.LoadAsset(assetName) as T;
    }

    /// <summary>
    /// 获取包里所有资源
    /// </summary>
    /// <returns></returns>
    public Object[] LoadAllAssets()
    {
        if (assetbundle == null)
        {
            Debug.LogError("当前资源包为空,无法获取资源!");
            return null;
        }
        else
            return assetbundle.LoadAllAssets();
    }

    /// <summary>
    /// 获取带有子物体的资源[图集等]
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns>所有资源</returns>
    public Object[] LoadAssetWithSubAssets(string assetName)
    {
        if (assetbundle == null)
        {
            Debug.LogError("当前资源包为空,无法获取" + assetName + "资源");
            return null;
        }
        else if (!assetbundle.Contains(assetName))
        {
            Debug.LogError("当前资源不包含" + assetName + "资源");
            return null;
        }
        else
            return assetbundle.LoadAssetWithSubAssets(assetName);
    }

    /// <summary>
    /// 卸载 assetbundle包
    /// </summary>
    /// <param name="obj">资源</param>
    public void UnloadAsset(Object obj)
    {
        Resources.UnloadAsset(obj);
    }

    /// <summary>
    /// 释放bundle包
    /// </summary>
    public void Dispose()
    {
        if (assetbundle == null)
            return;

        assetbundle.Unload(false);
        // false : 只卸载资源包
        // true :  卸载资源包和包里面的obj
    }

    /// <summary>
    /// 调试专用
    /// </summary>
    public void GetAllAssetNames()
    {
        string[] assetNames = assetbundle.GetAllAssetNames();
        foreach (string item in assetNames)
            Debug.Log(item);
    }
}