using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// 02
/// 加载 AssetBundle 包
/// </summary>
public class AssetBundleLoader
{
    /// <summary>
    /// 包里的资源[01对象]{上层定义一个下层的对象,来传递}
    /// </summary>
    private AssetLoader assetLoader;

    /// <summary>
    /// 包名(需要上层传递)
    /// </summary>
    private string bundleName;

    /// <summary>
    /// bundle包的路径
    /// </summary>
    private string bundlePath;

    /// <summary>
    /// 下载进度
    /// </summary>
    private float progress;

    /// <summary>
    /// 加载进度回调(需要上层传递)
    /// </summary>
    private LoadProgress lp;

    /// <summary>
    /// 加载完成回调(需要上层传递)
    /// </summary>
    private LoadComplete lc;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="lc"></param>
    /// <param name="lp"></param>
    /// <param name="bundleName"></param>
    public AssetBundleLoader(LoadComplete lc, LoadProgress lp, string bundleName)
    {
        this.lc = lc;
        this.lp = lp;
        this.bundleName = bundleName;
        this.progress = 0;

        this.bundlePath = string.Format("@{0}/{1}/{2}", Application.persistentDataPath, PlatformManager.Instance.Name(), bundleName);
        this.assetLoader = null;
    }


    /// <summary>
    /// 加载资源包
    /// </summary>
    /// <returns>www.assetBundle</returns>
    public void Load()
    {
        // 注 : 如果发布到手机端,就不需要www加载了,通过下面方式加载:
        //AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(bundlePath);
        //yield return abcr;
        UnityWebRequester requester = new UnityWebRequester();
        requester.GetAssetBundle(bundlePath, (AssetBundle ab) =>
        {
            // 创建 AssetLoader 类,给 assetBundle 赋值
            assetLoader = new AssetLoader(ab);
            lp?.Invoke(bundleName, requester.DownloadedProgress);
            lc?.Invoke(bundleName);
        });
        while (!requester.IsDown)
        {
            this.progress = requester.DownloadedProgress;
            lp?.Invoke(bundleName, progress);
        }
    }

    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns>Obj类型的资源</returns>
    public Object LoadAsset(string assetName)
    {
        if (assetLoader == null)
        {
            Debug.LogError("当前assetLoader为空!" + assetLoader);
            return null;
        }
        return assetLoader.LoadAsset(assetName);
    }

    /// <summary>
    /// 获取包里的所有资源
    /// </summary>
    /// <returns>obj类型的数组</returns>
    public Object[] LoadAllAssets()
    {
        if (assetLoader == null)
        {
            Debug.LogError("当前assetLoader为空!" + assetLoader);
            return null;
        }
        return assetLoader.LoadAllAssets();
    }

    /// <summary>
    /// 获取带有子物体的资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns>所有资源</returns>
    public Object[] LoadAssetWithSubAssets(string assetName)
    {
        if (assetLoader == null)
        {
            Debug.LogError("当前assetLoader为空!" + assetLoader);
            return null;
        }

        return assetLoader.LoadAssetWithSubAssets(assetName);
    }

    /// <summary>
    /// 卸载资源[Object类型]
    /// </summary>
    /// <param name="asset">obj</param>
    public void UnloadAsset(Object asset)
    {
        if (assetLoader == null)
        {
            Debug.LogError("当前assetLoader为空!" + assetLoader);
        }
        else
            assetLoader.UnloadAsset(asset);
    }

    /// <summary>
    /// 释放 bundle 包
    /// </summary>
    public void Dispose()
    {
        if (assetLoader == null)
            return;

        assetLoader.Dispose();
        // false : 只卸载包
        // true  : 卸载包和obj
        assetLoader = null; //释放资源后,把assetLoader置空
    }

    /// <summary>
    /// 调试专用
    /// </summary>
    public void GetAllAssetNames()
    {
        assetLoader.GetAllAssetNames();
    }
}