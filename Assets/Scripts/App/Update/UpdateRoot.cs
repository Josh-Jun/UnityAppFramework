using Data;
using EventController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Update
{
    public class UpdateRoot : SingletonEvent<UpdateRoot>, IRoot
    {
        private UpdateWindow window;

        #region Private Var
        private string LocalPath;
        private string XmlLocalVersionPath;

        private string ServerUrl;
        private string XmlServerVersionPath;

        private List<Scene> downloadScenes = new List<Scene>();
        private readonly List<Folder> alreadyDownLoadList = new List<Folder>();
        private Dictionary<Folder, UnityWebRequester> unityWebRequesterPairs = new Dictionary<Folder, UnityWebRequester>();
        private UnityWebRequester unityWebRequester;
        private AssetBundleConfig localABConfig;
        private AssetBundleConfig serverABConfig;
        #endregion

        #region Public Var
        public float LoadTotalSize { private set; get; } = 0; // 需要加载资源的总大小 KB
        /// <summary> 获取下载进度 </summary>
        public float GetProgress { get { return GetLoadedSize() / LoadTotalSize; } }
        #endregion
        public UpdateRoot()
        {
            AddEventMsg("UpdateNow", () => { UpdateNow(); });
        }
        public void Begin()
        {
            string prefab_UpdatePath = "App/Update/Windows/UpdateWindow";
            window = AssetsManager.Instance.LoadLocalUIWindow<UpdateWindow>(prefab_UpdatePath);
            window.SetWindowActive();

            window.SetTipsText("检查更新中...");
            window.SetProgressValue(0);
            StartUpdate((bool isUpdate, string des) =>
            {
                if (isUpdate)
                {
                    window.SetContentText(des);
                    window.SetUpdateTipsActive(true);
                }
                else
                {
                    LoadAssetBundle();
                }
            });
        }

        public void End()
        {
            
        }
        private void UpdateNow()
        {
            window.SetUpdateTipsActive(false);
            window.SetProgressBarActive(true);
            window.SetTipsText("下载中...");
            StartDownLoad();
            App.app.StartCoroutine(DownLoading());

        }
        private IEnumerator DownLoading()
        {
            float time = 0;
            float speed = 0;
            while (GetProgress != 1f)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
                speed = GetLoadedSize() / time;
                window.SetSpeedText(speed);
                window.SetProgressText(GetLoadedSize(), LoadTotalSize);
                window.SetProgressValue(GetProgress);
            }
            //更新下载完成，加载AB包
            LoadAssetBundle();
        }
        private void LoadAssetBundle()
        {
            window.SetTipsText("正在加载资源...");
            window.SetProgressBarActive(true);
            LoadAssetBundle((bool isEnd, string bundleName, float bundleProgress) =>
            {
                window.SetTipsText(string.Format("正在加载资源:{0}", bundleName));
                window.SetProgressValue(bundleProgress);
                if (isEnd && bundleProgress == 1)
                {
                    window.SetProgressBarActive(false);
                    window.SetWindowActive(false);
                    //AB包加载完成
                    Root.StartApp();
                }
            });
        }

        #region Public Function
        /// <summary> 开始热更新 </summary>
        private void StartUpdate(Action<bool, string> UpdateCallBack)
        {
            LocalPath = PlatformManager.Instance.GetDataPath(PlatformManager.Instance.Name) + "/";
            ServerUrl = NetcomManager.ABUrl + PlatformManager.Instance.Name + "/";

            XmlLocalVersionPath = LocalPath + "AssetBundleConfig.xml";
            XmlServerVersionPath = ServerUrl + "AssetBundleConfig.xml";

            if (FileManager.FileExist(XmlLocalVersionPath))
            {
                //读取本地热更文件
                DownLoad(XmlLocalVersionPath, (byte[] bytes) =>
                {
                    localABConfig = bytes != null ? XmlManager.ProtoDeSerialize<AssetBundleConfig>(bytes) : null;
                    //检查版本更新信息
                    CheckUpdate(localABConfig, () =>
                    {
                        UpdateCallBack?.Invoke(downloadScenes.Count > 0, serverABConfig.Des);
                    });
                });
            }
            else
            {
                //检查版本更新信息
                CheckUpdate(localABConfig, () =>
                {
                    UpdateCallBack?.Invoke(downloadScenes.Count > 0, serverABConfig.Des);
                });
            }
        }

        /// <summary> 下载AB包 </summary>
        private void StartDownLoad()
        {
            //下载总manifest文件
            DownLoad(ServerUrl + PlatformManager.Instance.Name + ".manifest", (byte[] manifest_data) =>
            {
                //将manifest文件写入本地
                FileManager.CreateFile(LocalPath + PlatformManager.Instance.Name + ".manifest", manifest_data);
                //下载总AB包
                DownLoad(ServerUrl + PlatformManager.Instance.Name, (byte[] ab_data) =>
                    {
                        //将ab文件写入本地
                        FileManager.CreateFile(LocalPath + PlatformManager.Instance.Name, ab_data);
                        DownLoadAssetBundle();
                    });
            });
        }
        /// <summary> 获取当前下载大小(M) </summary>
        private float GetLoadedSize()
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
        private void LoadAssetBundle(Action<bool, string, float> LoadCallBack)
        {
            int totalProgress = AssetBundleManager.Instance.ABScenePairs.Values.Sum(f => f.Count);
            int loadProgress = 0;
            AssetBundleManager.Instance.LoadAssetBundle((bundleName, progress) =>
            {
                if (progress == 1)
                {
                    loadProgress++;
                }
                LoadCallBack?.Invoke(totalProgress == loadProgress, bundleName, progress);
            });
        }
        /// <summary> 清理本地AB包和版本XML文件 </summary>
        private void DeleteLocalVersionXml()
        {
            if (FileManager.FileExist(XmlLocalVersionPath))
            {
                FileManager.DeleteFile(XmlLocalVersionPath);
                FileManager.DeleteFolder(LocalPath);
            }
        }
        #endregion

        #region Private Function
        private int _indexI;
        private int _indexJ;
        private void DownLoadAssetBundle()
        {
            unityWebRequesterPairs.Clear();
            for (int i = 0; i < downloadScenes.Count; i++)
            {
                _indexI = i;
                for (int j = 0; j < downloadScenes[i].Folders.Count; j++)
                {
                    _indexJ = j;
                    var folder = downloadScenes[_indexI].Folders[_indexJ];
                    // Debug.Log(folder.BundleName);
                    //添加到下载列表
                    if (unityWebRequesterPairs.ContainsKey(folder))
                        continue;
                    UnityWebRequester requester = new UnityWebRequester(App.app);
                    unityWebRequesterPairs.Add(folder, requester);
                    //下载manifest文件
                    DownLoad(ServerUrl + folder.BundleName + ".manifest", (byte[] _manifest_data) =>
                    {
                        //将manifest文件写入本地
                        FileManager.CreateFile(LocalPath + folder.BundleName + ".manifest", _manifest_data);
                    });
                }
            }
            foreach (var requester in unityWebRequesterPairs)
            {
                unityWebRequester = requester.Value;
                requester.Value.GetBytes(ServerUrl + requester.Key.BundleName, (byte[] data) =>
                {
                    FileManager.CreateFile(LocalPath + requester.Key.BundleName, data);
                    unityWebRequester = null;
                    alreadyDownLoadList.Add(requester.Key);
                    //下载完成后修改本地版本文件
                    UpdateLocalConfig(requester.Key);
                });
            }
        }
        private void UpdateLocalConfig(Folder folder)
        {
            if (!FileManager.FileExist(XmlLocalVersionPath))
            {
                AssetBundleConfig config = new AssetBundleConfig();
                config.GameVersion = serverABConfig.GameVersion;
                config.Platform = serverABConfig.Platform;
                config.Scenes = serverABConfig.Scenes;
                FileManager.CreateFile(XmlLocalVersionPath, XmlManager.ProtoByteSerialize<AssetBundleConfig>(config));

                UpdateConfig();
            }
            UpdateConfig(folder);
        }

        private void UpdateConfig(Folder folder = null)
        {
            XmlDocument xmlDocument = XmlManager.Load(XmlLocalVersionPath);
            var scene = xmlDocument.GetElementsByTagName("Scenes");
            for (int i = 0; i < scene.Count; i++)
            {
                var folders = scene[i].ChildNodes;
                for (int j = 0; j < folders.Count; j++)
                {
                    if (folder != null)
                    {
                        if (folder.BundleName == folders[j].Attributes["BundleName"].Value)
                        {
                            folders[j].Attributes["MD5"].Value = folder.MD5;
                            folders[j].Attributes["Size"].Value = folder.Size.ToString();
                        }
                    }
                    else
                    {
                        folders[j].Attributes["MD5"].Value = "";
                    }
                }
            }
            XmlManager.Save(xmlDocument, XmlLocalVersionPath);
        }

        /// <summary> 下载 </summary>
        private void DownLoad(string url, Action<byte[]> action)
        {
            UnityWebRequester requester = new UnityWebRequester(App.app);
            requester.GetBytes(url, (byte[] data) =>
            {
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
                    serverABConfig = XmlManager.ProtoDeSerialize<AssetBundleConfig>(bytes);
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
                AssetBundleManager.Instance.SetAbScenePairs(scene.SceneName, folderPairs);
            }

        }
        #endregion
        public void AppPause(bool pause)
        {
            
        }
        public void AppFocus(bool focus)
        {
            
        }
        public void AppQuit()
        {

        }
    }
}