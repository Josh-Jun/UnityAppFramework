using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 04
/// 加载 Manifest文件(记录了 bundle 包的依赖关系和被依赖关系)
/// </summary>
public class ManifestLoader : Singleton<ManifestLoader>
{
    /// <summary>
    /// Manifest 文件
    /// </summary>
    private AssetBundleManifest manifest;

    /// <summary>
    /// 路径
    /// </summary>
    private string manifestPath;

    public bool IsFinish { get; private set; }

    /// <summary>
    /// 全局存在的 AssetBundle
    /// </summary>
    private AssetBundle assetBundle;

    //private string asset = "AssetBundleManifest";


    public ManifestLoader()
    {
        this.manifestPath = string.Format("@{0}/{1}/{2}", Application.persistentDataPath, PlatformManager.Instance.Name(), PlatformManager.Instance.Name());
        this.manifest = null;
        this.assetBundle = null;
        this.IsFinish = false;
    }

    /// <summary>
    /// 开始加载 Manifest 文件
    /// </summary>
    /// <returns></returns>
    public void Load()
    {
        UnityWebRequester requester = new UnityWebRequester();
        requester.GetAssetBundle(manifestPath, (AssetBundle ab) =>
        {
            this.assetBundle = ab;
            this.manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            this.IsFinish = true;
        });
    }

    /// <summary>
    /// 获取一个 bundle 包的所有依赖关系[最重要]
    /// </summary>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    public string[] GetDependence(string bundleName)
    {
        return manifest.GetAllDependencies(bundleName);
    }

    /// <summary>
    /// 卸载 manifest
    /// </summary>
    public void UnLoad()
    {
        assetBundle.Unload(true);
    }
}