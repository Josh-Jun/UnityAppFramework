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

        private List<Folder> downloadFolders = new List<Folder>();
        private List<Folder> alreadlyFolders = new List<Folder>();
        private List<Folder> allDownloadFolders = new List<Folder>();

        private Dictionary<Folder, UnityWebRequester> unityWebRequesterPairs = new Dictionary<Folder, UnityWebRequester>();

        private AssetBundleConfig localABConfig;
        private AssetBundleConfig serverABConfig;
        private bool Downloading = false;
        private float TotalSize = 0; // 资源的总大小 MB
        private float AlreadlyLoadSize = 0; // 已经下载资源的总大小 KB

        private float alreadySize = 0;
        /// <summary> 获取下载大小(M) </summary>
        private float GetLoadedSize
        {
            get
            {
                return (AlreadlyLoadSize + alreadySize) / 1024f;
            }
        }

        /// <summary> 获取下载进度 </summary>
        private float GetProgress
        {
            get { return GetLoadedSize / TotalSize; }
        }

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
            window.StartCoroutine(DownLoading());
        }

        private IEnumerator DownLoading()
        {
            float time = 0;
            float previousSize = 0;
            float speed = 0;
            while (Downloading)
            {
                yield return new WaitForEndOfFrame();
                time += Time.deltaTime;
                if (time >= 1f)
                {
                    speed = (GetLoadedSize - previousSize);
                    previousSize = GetLoadedSize;
                    time = 0;
                }

                speed = speed > 0 ? speed : 0;
                window.SetSpeedText(speed);
                window.SetProgressText(GetLoadedSize, TotalSize);
                window.SetProgressValue(GetProgress);
            }

            window.SetProgressText(TotalSize, TotalSize);
            window.SetProgressValue(GetProgress);
            yield return new WaitForEndOfFrame();
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
                    TimerTaskManager.Instance.AddFrameTask(() => { Root.StartApp(); }, 1);
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
                DownLoad($"file://{XmlLocalVersionPath}", (byte[] bytes) =>
                {
                    localABConfig = bytes != null
                        ? XmlManager.ProtoDeSerialize<AssetBundleConfig>(bytes)
                        : null;
                    //检查版本更新信息
                    CheckUpdate(localABConfig, () => { UpdateCallBack?.Invoke(downloadFolders.Count > 0, serverABConfig.Des); });
                });
            }
            else
            {
                //检查版本更新信息
                CheckUpdate(localABConfig, () => { UpdateCallBack?.Invoke(downloadFolders.Count > 0, serverABConfig.Des); });
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
                    window.StartCoroutine(DownLoadAssetBundle());
                });
            });
        }

        /// <summary> 加载AB包 </summary>
        private void LoadAssetBundle(Action<bool, string, float> LoadCallBack)
        {
            int totalProgress = AssetBundleManager.Instance.ABScenePairs.Values.Sum(f => f.Count);
            int loadProgress = 0;
            AssetBundleManager.Instance.LoadAllAssetBundle((bundleName, progress) =>
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

        private bool Finished = true;

        private IEnumerator DownLoadAssetBundle()
        {
            unityWebRequesterPairs.Clear();
            for (int i = 0; i < downloadFolders.Count; i++)
            {
                var folder = downloadFolders[i];
                //添加到下载列表
                if (unityWebRequesterPairs.ContainsKey(folder))
                    continue;
                UnityWebRequester requester = new UnityWebRequester(window);
                unityWebRequesterPairs.Add(folder, requester);
                //下载manifest文件
                DownLoad(ServerUrl + folder.BundleName + ".manifest", (byte[] _manifest_data) =>
                {
                    //将manifest文件写入本地
                    FileManager.CreateFile(LocalPath + folder.BundleName + ".manifest", _manifest_data);
                });
            }

            Downloading = true;
            yield return new WaitForEndOfFrame();
            foreach (var requester in unityWebRequesterPairs)
            {
                yield return new WaitUntil(() => Finished);
                Finished = false;
                requester.Value.DownloadFile(ServerUrl + requester.Key.BundleName, LocalPath + requester.Key.BundleName, (progress) =>
                {
                    alreadySize = requester.Key.Size * progress;
                    if (progress >= 1)
                    {
                        if (!Finished)
                        {
                            //下载完成后修改本地版本文件
                            UpdateLocalConfig(requester.Key);
                            alreadySize = 0;
                            AlreadlyLoadSize += requester.Key.Size;
                            requester.Value.Destory();
                            Finished = true;
                        }
                    }
                });
            }
            yield return new WaitForEndOfFrame();
            Downloading = false;
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
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(XmlLocalVersionPath);

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

            xmlDocument.Save(XmlLocalVersionPath);
        }

        /// <summary> 下载 </summary>
        private void DownLoad(string url, Action<byte[]> action)
        {
            UnityWebRequester requester = new UnityWebRequester(window);
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
                    Debug.Log($"大版本比较 v{Application.version}/v{serverABConfig.GameVersion}");
                    if (Application.version == serverABConfig.GameVersion)
                    {
                        //无大版本更新，校验本地ab包
                        for (int i = 0; i < serverABConfig.Scenes.Count; i++)
                        {
                            for (int j = 0; j < serverABConfig.Scenes[i].Folders.Count; j++)
                            {
                                if (localABConfig == null)
                                {
                                    downloadFolders.Add(serverABConfig.Scenes[i].Folders[j]);
                                }
                                else
                                {
                                    if (localABConfig.Scenes[i].Folders[j].BundleName == serverABConfig.Scenes[i].Folders[j].BundleName)
                                    {
                                        if (localABConfig.Scenes[i].Folders[j].MD5 != serverABConfig.Scenes[i].Folders[j].MD5)
                                        {
                                            downloadFolders.Add(serverABConfig.Scenes[i].Folders[j]);
                                        }
                                        else
                                        {
                                            alreadlyFolders.Add(serverABConfig.Scenes[i].Folders[j]);
                                        }
                                    }
                                }

                                allDownloadFolders.Add(serverABConfig.Scenes[i].Folders[j]);
                            }
                        }

                        TotalSize = allDownloadFolders.Sum(s => s.Size) / 1024f;
                        AlreadlyLoadSize = alreadlyFolders.Sum(s => s.Size);
                        cb?.Invoke();
                    }
                    else
                    {
                        //大版本更新,下载新程序覆盖安装
                        Debug.Log("大版本更新,下载新程序覆盖安装");
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