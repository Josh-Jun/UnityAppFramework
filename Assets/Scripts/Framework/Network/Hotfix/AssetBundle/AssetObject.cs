using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 06
/// 对Obj的封装
/// </summary>
public class AssetObject
{
    /// <summary>
    /// 获取资源
    /// </summary>
    public List<Object> AssetList { get; }

    /// <summary>
    /// 构造函数 (添加资源)
    /// </summary>
    /// <param name="assets"></param>
    public AssetObject(params Object[] assets)
    {
        AssetList = new List<Object>();
        AssetList.AddRange(assets);
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public void UnLoadAsset()
    {
        for (int i = AssetList.Count - 1; i >= 0; i--)
        {
            Resources.UnloadAsset(AssetList[i]);
        }
    }
}