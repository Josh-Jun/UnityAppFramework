using System;
using UnityEngine;
using Data;
using System.Collections.Generic;

/// <summary>
/// 热更管理者
/// </summary>
public class HotfixManager : SingletonEvent<HotfixManager>
{
    private string LocalPath;
    private string XmlLocalVersionPath;

    private string ServerUrl;
    private string XmlServerVersionPath;

    public Dictionary<string, Dictionary<string, string>> ScenePairs { get; private set; } = new Dictionary<string, Dictionary<string, string>>();

    public float LoadTotalSize { private set; get; } = 0; // 需要加载资源的总大小 KB
    public float LoadedSize { private set; get; } = 0;    // 已经下载资源大小 KB
    //开始热更新
    public void StartHotfix(Action<bool, List<Scene>> HotfixCallBack)
    {
        LocalPath = string.Format("@{0}/{1}/", Application.persistentDataPath, PlatformManager.Instance.Name());
        ServerUrl = NetcomManager.URL + PlatformManager.Instance.Name() + "/";

        XmlLocalVersionPath = LocalPath + "AssetBundleConfig.xml";
        XmlServerVersionPath = ServerUrl + "AssetBundleConfig.xml";

        if (!FileManager.FileExist(XmlLocalVersionPath))
        {
            //本地无版本，全部更新
            DownLoad(XmlServerVersionPath, (byte[] data) =>
             {
                 AssetBundleConfig abConfig = XmlSerializeManager.ProtoDeSerialize<AssetBundleConfig>(data);
                 FileManager.CreateFile(XmlLocalVersionPath, data);//将xml文件写入本地

                 ComputeLoadSize(abConfig);

                 HotfixCallBack?.Invoke(true, abConfig.Scenes);
             });
        }
        else
        {
            //获取本地版本信息，判断是否热更
            DownLoad(XmlLocalVersionPath, (byte[] bytes) =>
            {
                AssetBundleConfig localABConfig = XmlSerializeManager.ProtoDeSerialize<AssetBundleConfig>(bytes);

                ComputeLoadSize(localABConfig);

                //检查版本更新信息
                CheckHotfix(localABConfig, (scenes) =>
                {
                    if (scenes.Count > 0)
                    {
                        HotfixCallBack?.Invoke(true, scenes);
                    }
                    else
                    {
                        HotfixCallBack?.Invoke(false, null);
                    }
                });
            });
        }
    }
    /// <summary>
    /// 检查更新
    /// </summary>
    /// <param name="localABConfig"></param>
    /// <param name="cb"></param>
    private void CheckHotfix(AssetBundleConfig localABConfig, Action<List<Scene>> cb = null)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //提示网络错误，检测网络链接是否正常
            Debug.Log("网络异常");
        }
        else
        {
            DownLoad(XmlServerVersionPath, (byte[] bytes) =>
            {
                AssetBundleConfig serverABConfig = XmlSerializeManager.ProtoDeSerialize<AssetBundleConfig>(bytes);
                if (Application.version == serverABConfig.GameVersion)
                {
                    //无大版本更新，校验本地ab包
                    CheckScenesVersion(localABConfig, (scenes) =>
                    {
                        cb?.Invoke(scenes);
                    });
                }
                else
                {
                    //大版本更新,下载新程序覆盖安装

                }
            });
        }
    }
    /// <summary>
    /// 校验ab包
    /// </summary>
    /// <param name="localABConfig"></param>
    /// <param name="cb"></param>
    private void CheckScenesVersion(AssetBundleConfig localABConfig, Action<List<Scene>> cb)
    {
        List<Scene> scenes = new List<Scene>();
        DownLoad(XmlServerVersionPath, (byte[] data) =>
        {
            AssetBundleConfig serverABConfig = XmlSerializeManager.ProtoDeSerialize<AssetBundleConfig>(data);
            for (int i = 0; i < localABConfig.Scenes.Count; i++)
            {
                for (int j = 0; j < localABConfig.Scenes[i].Folders.Count; j++)
                {
                    if (localABConfig.Scenes[i].Folders[j].HashCode == serverABConfig.Scenes[i].Folders[j].HashCode)
                    {
                        continue;
                    }
                    else
                    {
                        scenes.Add(serverABConfig.Scenes[i]);
                        break;
                    }
                }
            }
            cb?.Invoke(scenes);
        });
    }
    /// <summary>
    /// 下载
    /// </summary>
    /// <param name="url"></param>
    /// <param name="action"></param>
    private void DownLoad(string url, Action<byte[]> action)
    {
        NetcomManager.Instance.GetBytes(url, (NetcomData data) =>
        {
            if (data.isDown)
            {
                if (!data.isError)
                {
                    action?.Invoke(data.data);
                }
                else
                {
                    Debug.LogErrorFormat("[HotfixManager] Error : {0}", data.error);
                }
            }
        });
    }
    /// <summary>
    /// 下载AB包
    /// </summary>
    /// <param name="scenes"></param>
    public void DownLoadAssetBundle(List<Scene> scenes, Action<float> DownCallBack)
    {
        //下载总manifest文件
        DownLoad(ServerUrl + PlatformManager.Instance.Name() + ".manifest", (byte[] manifest_data) =>
        {
            //将manifest文件写入本地
            FileManager.CreateFile(LocalPath + PlatformManager.Instance.Name() + ".manifest", manifest_data);
            //下载总AB包
            DownLoad(ServerUrl + PlatformManager.Instance.Name(), (byte[] ab_data) =>
            {
                //将ab文件写入本地
                FileManager.CreateFile(LocalPath + PlatformManager.Instance.Name(), ab_data);
                for (int i = 0; i < scenes.Count; i++)
                {
                    for (int j = 0; j < scenes[i].Folders.Count; j++)
                    {
                        string bandleName = scenes[i].Folders[j].BundleName;
                        float size = scenes[i].Folders[j].Size;
                        //下载manifest文件
                        DownLoad(ServerUrl + bandleName + ".manifest", (byte[] _manifest_data) =>
                        {
                            //将manifest文件写入本地
                            FileManager.CreateFile(LocalPath + bandleName + ".manifest", _manifest_data);
                            //下载ab包
                            NetcomManager.Instance.GetBytes(ServerUrl + bandleName, (NetcomData netcom) =>
                            {
                                float LoadingSize = netcom.progress * size / 1024f;
                                if (netcom.progress == 1)
                                {
                                    if (!netcom.isError)
                                    {
                                        //将ab文件写入本地
                                        FileManager.CreateFile(LocalPath + bandleName, netcom.data);
                                    }
                                    LoadedSize += LoadingSize;
                                    LoadingSize = 0;
                                }
                                if (LoadedSize == LoadTotalSize)
                                {
                                    DownCallBack?.Invoke((LoadedSize + LoadingSize) / LoadTotalSize);
                                }
                            });
                        });
                    }
                }
            });
        });
    }
    /// <summary>
    /// 保存场景中的文件夹和包名
    /// </summary>
    /// <param name="abConfig"></param>
    private void ComputeLoadSize(AssetBundleConfig abConfig)
    {
        LoadTotalSize = 0;
        //获取文件夹名和包名，用来给AssetbundleSceneManager里的folderDic赋值
        foreach (var scene in abConfig.Scenes)
        {
            Dictionary<string, string> folderPairs = new Dictionary<string, string>();
            foreach (var folder in scene.Folders)
            {
                if (!folderPairs.ContainsKey(folder.FolderName))
                {
                    folderPairs.Add(folder.FolderName, folder.BundleName);
                }
                LoadTotalSize += folder.Size / 1024f;
            }
            if (!ScenePairs.ContainsKey(scene.SceneName))
            {
                ScenePairs.Add(scene.SceneName, folderPairs);
            }
        }
    }
    /// <summary>
    /// 加载AB包
    /// </summary>
    public void LoadAssetBundle(Action<bool, string, float> LoadCallBack)
    {
        int totalProgress = 0;
        int loadProgress = 0;
        foreach (var scene in ScenePairs)
        {
            foreach (var folder in scene.Value)
            {
                totalProgress++;
            }
        }
        foreach (var scene in ScenePairs)
        {
            foreach (var folder in scene.Value)
            {
                AssetBundleManager.Instance.LoadAssetBundle(scene.Key, folder.Key, (bundleName, progress) =>
                {
                    if(progress == 1)
                    {
                        loadProgress++;
                    }
                    LoadCallBack?.Invoke(totalProgress == loadProgress, bundleName, progress);
                });
            }
        }
    }
    /// <summary>
    /// 清理本地AB包和版本XML文件
    /// </summary>
    public void DeleteLocalVersionXml()
    {
        if (FileManager.FileExist(XmlLocalVersionPath))
        {
            FileManager.DeleteFile(XmlLocalVersionPath);
            FileManager.DeleteFolder(LocalPath);
        }
    }
}
