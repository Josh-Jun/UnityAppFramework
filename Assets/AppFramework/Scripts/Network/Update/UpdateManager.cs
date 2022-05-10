using System;
using UnityEngine;
using Data;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using System.Collections;
using System.Xml;

/// <summary>
/// 热更管理者
/// </summary>
public class UpdateManager : SingletonEvent<UpdateManager>
{
    #region Private Var
    private string LocalPath;
    private string XmlLocalVersionPath;

    private string ServerUrl;
    private string XmlServerVersionPath;

    private List<Scene> downloadScenes = new List<Scene>();
    private readonly List<Folder> alreadyDownLoadList = new List<Folder>();
    private Dictionary<Folder, UnityWebRequester> unityWebRequesterPairs = new Dictionary<Folder, UnityWebRequester>();
    private UnityWebRequester unityWebRequester;
    private byte[] bytes = null;
    private AssetBundleConfig localABConfig;
    private AssetBundleConfig serverABConfig;
    #endregion

    #region Public Var
    public Dictionary<string, Dictionary<string, string>> ABScenePairs { get; private set; } = new Dictionary<string, Dictionary<string, string>>();
    public float LoadTotalSize { private set; get; } = 0; // 需要加载资源的总大小 KB
    #endregion

    #region Public Function
    /// <summary> 开始热更新 </summary>
    public void StartUpdate(Action<bool, string> UpdateCallBack)
    {
        LocalPath = PlatformManager.Instance.GetDataPath(PlatformManager.Instance.Name());
        ServerUrl = NetcomManager.ABUrl + PlatformManager.Instance.Name() + "/";

        XmlLocalVersionPath = LocalPath + "AssetBundleConfig.xml";
        XmlServerVersionPath = ServerUrl + "AssetBundleConfig.xml";

        //读取本地热更文件
        DownLoad(XmlLocalVersionPath, (byte[] bytes) =>
        {
            localABConfig = bytes != null ? XmlSerializeManager.ProtoDeSerialize<AssetBundleConfig>(bytes) : null;
            //检查版本更新信息
            CheckUpdate(localABConfig, () =>
            {
                string des = "";
                for (int i = 0; i < downloadScenes.Count; i++)
                {
                    des += downloadScenes[i].Des + "\n";
                }
                UpdateCallBack?.Invoke(downloadScenes.Count > 0, des);
            });
        });
    }

    /// <summary> 下载AB包 </summary>
    public void DownLoadAssetBundle()
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
                App.app.StartCoroutine(IE_DownLoad());
            });
        });
    }
    /// <summary> 获取下载进度 </summary>
    public float GetProgress()
    {
        return GetLoadedSize() / LoadTotalSize;
    }
    /// <summary> 获取当前下载大小(M) </summary>
    public float GetLoadedSize()
    {
        float alreadySize = alreadyDownLoadList.Sum(f => f.Size);
        float currentAlreadySize = 0;
        if (unityWebRequester != null)
        {
            Folder folder = unityWebRequesterPairs.FirstOrDefault(r => r.Value == unityWebRequester).Key;
            currentAlreadySize = unityWebRequester.DownloadedProgress * folder.Size;
        }
        return (alreadySize + currentAlreadySize) / 1024f;
    }
    /// <summary> 加载AB包 </summary>
    public void LoadAssetBundle(Action<bool, string, float> LoadCallBack)
    {
        int totalProgress = ABScenePairs.Values.Sum(f => f.Count);
        int loadProgress = 0;
        foreach (var scene in ABScenePairs)
        {
            foreach (var folder in scene.Value)
            {
                AssetBundleManager.Instance.LoadAssetBundle(scene.Key, folder.Key, (bundleName, progress) =>
                {
                    if (progress == 1)
                    {
                        loadProgress++;
                    }
                    LoadCallBack?.Invoke(totalProgress == loadProgress, bundleName, progress);
                });
            }
        }
    }
    /// <summary> 清理本地AB包和版本XML文件 </summary>
    public void DeleteLocalVersionXml()
    {
        if (FileManager.FileExist(XmlLocalVersionPath))
        {
            FileManager.DeleteFile(XmlLocalVersionPath);
            FileManager.DeleteFolder(LocalPath);
        }
    }
    #endregion

    #region Private Function
    private IEnumerator IE_DownLoad()
    {
        unityWebRequesterPairs.Clear();
        for (int i = 0; i < downloadScenes.Count; i++)
        {
            for (int j = 0; j < downloadScenes[i].Folders.Count; j++)
            {
                string bandleName = downloadScenes[i].Folders[j].BundleName;
                float size = downloadScenes[i].Folders[j].Size;
                //添加到下载列表
                unityWebRequesterPairs.Add(downloadScenes[i].Folders[j], new UnityWebRequester(App.app));
                //下载manifest文件
                DownLoad(ServerUrl + bandleName + ".manifest", (byte[] _manifest_data) =>
                {
                    //将manifest文件写入本地
                    FileManager.CreateFile(LocalPath + bandleName + ".manifest", _manifest_data);
                });
            }
        }
        foreach (var requester in unityWebRequesterPairs)
        {
            unityWebRequester = requester.Value;
            yield return requester.Value.IE_Get(ServerUrl + requester.Key.BundleName, (UnityWebRequest uwr) =>
            {
                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    FileManager.CreateFile(LocalPath + requester.Key.BundleName, uwr.downloadHandler.data);
                    unityWebRequester = null;
                    alreadyDownLoadList.Add(requester.Key);
                    //下载完成后修改本地版本文件
                    UpdateLocalConfig(requester.Key);
                }
                else
                {
                    Debug.LogErrorFormat("[HotfixManager] Error:{0}", uwr.error);
                }
            });
        }
        FileManager.CreateFile(XmlLocalVersionPath, bytes);
    }
    private void UpdateLocalConfig(Folder folder)
    {
        if (!FileManager.FileExist(XmlLocalVersionPath))
        {
            AssetBundleConfig config = new AssetBundleConfig();
            FileManager.CreateFile(XmlLocalVersionPath, XmlSerializeManager.ProtoByteSerialize<AssetBundleConfig>(config));
        }
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(XmlLocalVersionPath);

        var scene = xmlDocument.GetElementsByTagName("Scenes");
        for (int i = 0; i < scene.Count; i++)
        {
            var folders = scene[i].ChildNodes;
            for (int j = 0; j < folders.Count; j++)
            {
                if (folder.FolderName == folders[j].Attributes["FolderName"].Value)
                {
                    folders[j].Attributes["HashCode"].Value = folder.MD5;
                    folders[j].Attributes["Size"].Value = folder.Size.ToString();
                }
            }
        }

        xmlDocument.Save(XmlLocalVersionPath);
    }
    /// <summary> 下载 </summary>
    private void DownLoad(string url, Action<byte[]> action)
    {
        UnityWebRequester requester = new UnityWebRequester(App.app);
        requester.Get(url, (UnityWebRequest uwr) =>
        {
            byte[] data = uwr.result == UnityWebRequest.Result.Success ? uwr.downloadHandler.data : null;
            action?.Invoke(data);
            requester.Destory();
        });
    }
    /// <summary> 检查更新 </summary>
    private void CheckUpdate(AssetBundleConfig localABConfig, Action cb = null)
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
                serverABConfig = XmlSerializeManager.ProtoDeSerialize<AssetBundleConfig>(bytes);
                this.bytes = bytes;
                GetABScenePairs(serverABConfig);
                if (Application.version == serverABConfig.GameVersion)
                {
                    //无大版本更新，校验本地ab包
                    downloadScenes = localABConfig != null ? new List<Scene>() : serverABConfig.Scenes;
                    if (localABConfig != null)
                    {
                        for (int i = 0; i < localABConfig.Scenes.Count; i++)
                        {
                            for (int j = 0; j < localABConfig.Scenes[i].Folders.Count; j++)
                            {
                                if (localABConfig.Scenes[i].Folders[j].MD5 != serverABConfig.Scenes[i].Folders[j].MD5)
                                {
                                    downloadScenes.Add(serverABConfig.Scenes[i]);
                                }
                            }
                        }
                    }
                    LoadTotalSize = downloadScenes.Sum(s => s.Folders.Sum(f => f.Size)) / 1024f;
                    cb?.Invoke();
                }
                else
                {
                    //大版本更新,下载新程序覆盖安装

                }
            });
        }
    }
    /// <summary> 获取文件夹名和包名 </summary>
    private void GetABScenePairs(AssetBundleConfig config)
    {
        //获取文件夹名和包名，用来给AssetbundleSceneManager里的folderDic赋值
        foreach (var scene in config.Scenes)
        {
            Dictionary<string, string> folderPairs = new Dictionary<string, string>();
            foreach (var folder in scene.Folders)
            {
                if (!folderPairs.ContainsKey(folder.FolderName))
                {
                    folderPairs.Add(folder.FolderName, folder.BundleName);
                }
            }
            if (!ABScenePairs.ContainsKey(scene.SceneName))
            {
                ABScenePairs.Add(scene.SceneName, folderPairs);
            }
        }
    }
    #endregion
}
