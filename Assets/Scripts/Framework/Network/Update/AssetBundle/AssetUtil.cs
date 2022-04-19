/// <summary>
/// 加载进度
/// </summary>
/// <param name="bundleName">包名</param>
/// <param name="progress">加载进度</param>
public delegate void LoadProgress(string bundleName, float progress);

/// <summary>
/// 加载完成时调用
/// </summary>
/// <param name="bundleName">包名</param>
public delegate void LoadComplete(string bundleName);

/// <summary>
/// 加载assetbundle的回调
/// </summary>
/// <param name="sceneName"></param>
/// <param name="bundleName"></param>
public delegate void LoadAssetBundleCallback(string sceneName, string bundleName);

public class AssetUtil
{

}