using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
/// <summary>
/// 场景里面所有包的管理器
/// 转换包名[文件夹名 -> 包名]
/// 当时一键做标记的时候,怎么写入的现在怎么读出来
/// </summary>
public class AssetbundleSceneManager
{
    /// <summary>
    /// 一个场景里面所有的资源包
    /// </summary>
    private AssetbundleScene sceneAssetBundles;

    /// <summary>
    /// 文件夹名和包名的映射
    /// </summary>
    private Dictionary<string, string> folderDic;

    /// <summary>
    ///  构造
    /// </summary>
    /// <param name="sceneName"></param>
    public AssetbundleSceneManager(string sceneName)
    {
        sceneAssetBundles = new AssetbundleScene(sceneName);
        folderDic = new Dictionary<string, string>();
    }

    #region 自身的功能
    /// <summary>
    /// 是否加载了这个文件夹
    /// </summary>
    /// <param name="folderName">文件夹名</param>
    /// <returns></returns>
    public bool IsLoading(string folderName)
    {
        if (folderDic.ContainsKey(folderName))
        {
            // 获取包名[根据文件夹名]
            string bundleName = folderDic[folderName];
            // 开始加载
            return sceneAssetBundles.IsLoading(bundleName);
        }
        else
        {
            Debug.LogError("找不到这个文件夹" + folderName + "对应的包!");
            return false;
        }

    }

    /// <summary>
    /// 是否加载完成这个包
    /// </summary>
    /// <param name="folderName">文件夹名</param>
    /// <returns></returns>
    public bool IsFinished(string folderName)
    {
        if (folderDic.ContainsKey(folderName))
        {
            // 获取包名[根据文件夹名]
            string bundleName = folderDic[folderName];
            // 开始加载
            return sceneAssetBundles.IsFinished(bundleName);
        }
        else
        {
            Debug.LogError("找不到这个文件夹" + folderName + "对应的包!");
            return false;
        }
    }

    /// <summary>
    /// 读取记录文件
    /// </summary>
    /// <param name="sceneName"></param>
    public void ReadRecord(string sceneName)
    {
        //string path = PlatformManager.Instance.ABPath() + PlatformManager.Instance.Name() + "/" + sceneName + "Record.txt";
        //using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        //{
        //    using (StreamReader sr = new StreamReader(fs))
        //    {
        //        int count = int.Parse(sr.ReadLine());
        //        for (int i = 0; i < count; i++)
        //        {
        //            string line = sr.ReadLine();
        //            string[] kv = line.Split(new string[] { "--" }, System.StringSplitOptions.None);

        //            folderDic.Add(kv[0], kv[1]);
        //        }
        //    }
        //}
        folderDic = HotfixManager.Instance.ABScenePairs[sceneName];
    }

    /// <summary>
    /// 通过文件夹名来获取完整包名
    /// </summary>
    /// <param name="folderName">文件夹名</param>
    /// <returns></returns>
    public string GetBundleName(string folderName)
    {

        if (!folderDic.ContainsKey(folderName))
        {
            Debug.LogError("没有这个文件夹!" + folderName);
            return null;
        }

        return folderDic[folderName];

    }
    #endregion

    #region 下层 API
    /// <summary>
    /// 加载资源包(需要上层提供文件夹名,这里会根据文件夹名获取包名,然后加载)
    /// </summary>
    /// <param name="folderName">文件夹名</param>
    /// <param name="lp"></param>
    /// <param name="labcb"></param>
    public void LoadAssetBundle(string folderName, LoadProgress lp, LoadAssetBundleCallback labcb)
    {
        if (folderDic.ContainsKey(folderName))
        {
            // 获取包名[根据文件夹名]
            string bundleName = folderDic[folderName];
            // 开始加载
            sceneAssetBundles.LoadAssetBundle(bundleName, lp, labcb);
        }
        else
        {
            Debug.LogError("找不到这个文件夹" + folderName + "对应的包!");
        }
    }

    /// <summary>
    /// 加载包
    /// </summary>
    /// <param name="bundleName">包名</param>
    /// <returns></returns>
    public IEnumerator Load(string bundleName)
    {
        yield return sceneAssetBundles.Load(bundleName);
    }

    /// <summary>
    /// 获取单个资源
    /// </summary>
    /// <param name="assetName">资源名字</param>
    /// <returns>Obj类型的资源</returns>
    public Object LoadAsset(string folderName, string assetName)
    {
        if (sceneAssetBundles == null)
        {
            Debug.LogError("当前sceneAssetBundles为空,无法获取该" + assetName + "资源");
            return null;
        }
        if (folderDic.ContainsKey(folderName))
        {
            string bundleName = folderDic[folderName];
            return sceneAssetBundles.LoadAsset(bundleName, assetName);
        }
        else
        {
            Debug.LogError("找不到该文件夹!" + folderName);
            return null;
        }
    }

    /// <summary>
    /// 获取包里的所有资源
    /// </summary>
    /// <returns></returns>
    public Object[] LoadAllAssets(string folderName)
    {
        if (sceneAssetBundles == null)
        {
            Debug.LogError("当前sceneAssetBundles为空,无法获取该资源");
            return null;
        }
        if (folderDic.ContainsKey(folderName))
        {
            string bundleName = folderDic[folderName];
            return sceneAssetBundles.LoadAllAssets(bundleName);
        }
        else
        {
            Debug.LogError("找不到该文件夹!" + folderName);
            return null;
        }
    }

    /// <summary>
    /// 获取带有子物体的资源
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns>所有资源</returns>
    public Object[] LoadAssetWithSubAssets(string folderName, string assetName)
    {
        if (sceneAssetBundles == null)
        {
            Debug.LogError("当前sceneAssetBundles为空,无法获取该" + assetName + "资源");
            return null;
        }
        if (folderDic.ContainsKey(folderName))
        {
            string bundleName = folderDic[folderName];
            return sceneAssetBundles.LoadAssetWithSubAssets(bundleName, assetName);
        }
        else
        {
            Debug.LogError("找不到该文件夹!" + folderName);
            return null;
        }
    }

    /// <summary>
    /// 卸载一个包里的单个资源
    /// </summary>
    /// <param name="asset"></param>
    public void UnloadAsset(string folderName, string assetName)
    {

        if (folderDic.ContainsKey(folderName))
        {
            string bundleName = folderDic[folderName];
            sceneAssetBundles.UnloadAsset(bundleName, assetName);
        }
        else
        {
            Debug.LogError("找不到该文件夹!" + folderName + "对应的包!");
        }

    }

    /// <summary>
    /// 卸载一个包里的所有资源
    /// </summary>
    /// <param name="bundleName"></param>
    public void UnLoadAllAssets(string folderName)
    {
        if (folderDic.ContainsKey(folderName))
        {
            string bundleName = folderDic[folderName];
            sceneAssetBundles.UnLoadAllAssets(bundleName);
        }
        else
        {
            Debug.LogError("找不到该文件夹!" + folderName + "对应的包!");
        }
    }

    /// <summary>
    /// 卸载所有的资源
    /// </summary>
    public void UnLoadAll()
    {
        sceneAssetBundles.UnLoadAll();
    }

    /// <summary>
    /// 释放单个 bundle 包
    /// </summary>
    public void Dispose(string folderName)
    {
        if (folderDic.ContainsKey(folderName))
        {
            string bundleName = folderDic[folderName];
            sceneAssetBundles.Dispose(bundleName);
        }
        else
        {
            Debug.LogError("找不到该文件夹!" + folderName + "对应的包!");
        }
    }

    /// <summary>
    /// 卸载所有 bundle 包
    /// </summary>
    public void DisposeAll()
    {
        sceneAssetBundles.DisposeAll();
    }

    /// <summary>
    /// 包和资源全部卸载
    /// </summary>
    public void DisposeAndUnLoadAll()
    {
        sceneAssetBundles.DisposeAndUnLoadAll();
    }

    /// <summary>
    /// 调试专用
    /// </summary>
    public void GetAllAssetNames(string folderName)
    {
        if (folderDic.ContainsKey(folderName))
        {
            string bundleName = folderDic[folderName];
            sceneAssetBundles.Dispose(bundleName);
        }
        else
        {
            Debug.LogError("找不到该文件夹!" + folderName + "对应的包!");
        }
    }
    #endregion
}