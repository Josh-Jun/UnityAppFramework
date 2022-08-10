using Data;
using EventController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Ask;
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

        private string appUrl;
        private string appPath;
        
        private Dictionary<string, Folder> localFolders = new Dictionary<string, Folder>();
        private Dictionary<string, Folder> serverFolders = new Dictionary<string, Folder>();
        
        private List<Folder> downloadFolders = new List<Folder>();
        private List<Folder> alreadlyFolders = new List<Folder>();
        
        private AssetBundleConfig localABConfig;
        private AssetBundleConfig serverABConfig;
        
        private float TotalSize = 0; // 资源的总大小 MB
        private long alreadyDownloadSize = 0;
        private long downloadingSize;

        private UpdateMold UpdateMold = UpdateMold.None;
        /// <summary> 获取下载大小(M) </summary>
        private float GetLoadedSize
        {
            get
            {
                return (alreadyDownloadSize + downloadingSize) / 1024f / 1024f;
            }
        }

        /// <summary> 获取下载进度 </summary>
        private float GetProgress
        {
            get
            {
                return GetLoadedSize / TotalSize;
            }
        }

        #endregion

        public UpdateRoot()
        {
            AddEventMsg("UpdateNow", () => { UpdateNow(); });//
        }

        public void Begin()
        {
            LocalPath = PlatformManager.Instance.GetDataPath(PlatformManager.Instance.Name) + "/";
            ServerUrl = NetcomManager.ABUrl + PlatformManager.Instance.Name + "/";

            XmlLocalVersionPath = LocalPath + "AssetBundleConfig.xml";
            XmlServerVersionPath = ServerUrl + "AssetBundleConfig.xml";

            appUrl = NetcomManager.AppUrl + "meta.apk";
            appPath = PlatformManager.Instance.GetDataPath("App/meta.apk");
            
            string prefab_UpdatePath = "App/Update/Windows/UpdateWindow";
            window = AssetsManager.Instance.LoadLocalUIWindow<UpdateWindow>(prefab_UpdatePath);
            window.SetWindowActive();

            window.SetTipsText("检查更新中...");
            window.SetProgressValue(0);
            StartUpdate((UpdateMold mold, string des) =>
            {
                UpdateMold = mold;
                Debug.Log($"更新结果:{mold}");
                switch (mold)
                {
                    case UpdateMold.Hotfix:
                        window.SetContentText(des);
                        window.SetUpdateTipsActive(true);
                        break;
                    case UpdateMold.App:
                        window.SetContentText($"发现新版本应用:v{serverABConfig.GameVersion}");
                        window.SetUpdateTipsActive(true);
                        break;
                    case UpdateMold.None:
                        window.SetWindowActive(false);
                        Root.StartApp();
                        break;
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
            window.StartCoroutine(DownLoading());
        }

        private IEnumerator DownLoading()
        {
            float time = 0;
            float previousSize = 0;
            float speed = 0;
            switch (UpdateMold)
            {
                case UpdateMold.Hotfix:
                    StartDownLoad();
                    while (GetProgress < 1)
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
                    //更新下载完成，开始运行App
                    window.SetWindowActive(false);
                    Root.StartApp();
                    break;
                case UpdateMold.App:
                    #region 断点续传
                    // if (!PlayerPrefs.HasKey("APP_DOWNLOADING"))
                    // {
                    //     if (FileManager.FileExist(appPath))
                    //     {
                    //         FileManager.DeleteFile(appPath);
                    //     }
                    //     PlayerPrefs.SetString("APP_DOWNLOADING", DateTime.Now.ToString());
                    // }
                    // UnityWebRequester requester = new UnityWebRequester(window);
                    // requester.DownloadFile(appUrl, appPath, (size, progress) =>
                    // {
                    //     float total = size / 1024f / 1024f;
                    //     float downloading = progress * total;
                    //     time += Time.deltaTime;
                    //     if (time >= 1f)
                    //     {
                    //         speed = (downloading - previousSize);
                    //         previousSize = downloading;
                    //         time = 0;
                    //     }
                    //     
                    //     speed = speed > 0 ? speed : 0;
                    //     window.SetSpeedText(speed);
                    //     window.SetProgressText(downloading, total);
                    //     window.SetProgressValue(progress);
                    //     if (progress == 1)
                    //     {
                    //         if (PlayerPrefs.HasKey("APP_DOWNLOADING"))
                    //         {
                    //             PlayerPrefs.DeleteKey("APP_DOWNLOADING");
                    //         }
                    //         PlatformManager.Instance.InstallApp(appPath);
                    //     }
                    // });
                    #endregion
                    #region 正常下载
                    if (FileManager.FileExist(appPath))
                    {
                        FileManager.DeleteFile(appPath);
                    }
                    UnityWebRequester requester = new UnityWebRequester(window);
                    requester.GetBytes(appUrl, (bytes) =>
                    {
                        FileManager.CreateFile(appPath, bytes);
                        TimerManager.Instance.EndTimer(timeId);
                        PlatformManager.Instance.InstallApp(appPath);
                    });
                    timeId = TimerManager.Instance.StartTimer((t) =>
                    {
                        var size = requester.DownloadedLength / 1024f / 1024f;
                        time += Time.deltaTime;
                        if (time >= 1f)
                        {
                            speed = (size - previousSize);
                            previousSize = size;
                            time = 0;
                        }
                        speed = speed > 0 ? speed : 0;
                        window.SetSpeedText(speed);
                        window.SetProgressText(size, size / requester.DownloadedProgress);
                        window.SetProgressValue(requester.DownloadedProgress);
                    });
                    #endregion
                    break;
                case UpdateMold.None:
                    break;
            }
        }

        #region Public Function

        /// <summary> 开始热更新 </summary>
        private void StartUpdate(Action<UpdateMold, string> UpdateCallBack)
        {
            if (FileManager.FileExist(XmlLocalVersionPath))
            {
                //读取本地热更文件
                DownLoad($"file://{XmlLocalVersionPath}", (byte[] bytes) =>
                {
                    localABConfig = bytes != null
                        ? XmlManager.ProtoDeSerialize<AssetBundleConfig>(bytes)
                        : null;
                    //检查版本更新信息
                    CheckUpdate(localABConfig, (mold) => { UpdateCallBack?.Invoke(mold, serverABConfig.Des); });
                });
            }
            else
            {
                //检查版本更新信息
                CheckUpdate(localABConfig, (mold) => { UpdateCallBack?.Invoke(mold, serverABConfig.Des); });
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
        #endregion

        #region Private Function

        private bool Finished = true;
        private int timeId = -1;
        private IEnumerator DownLoadAssetBundle()
        {
            for (int i = 0; i < downloadFolders.Count; i++)
            {
                var folder = downloadFolders[i];

                if (localFolders.Count > 0)
                {
                    if (localFolders.ContainsKey(folder.BundleName))
                    {
                        #region 断点续传
                        // if (localFolders[folder.BundleName].Tag == "0")
                        // {
                        //     if (FileManager.FileExist(LocalPath + folder.BundleName))
                        //     {
                        //         FileManager.DeleteFile(LocalPath + folder.BundleName);
                        //     }
                        // }
                        // else if (localFolders[folder.BundleName].Tag == "1")
                        // {
                        //     if (localFolders[folder.BundleName].MD5 != folder.MD5)
                        //     {
                        //         if (FileManager.FileExist(LocalPath + folder.BundleName))
                        //         {
                        //             FileManager.DeleteFile(LocalPath + folder.BundleName);
                        //         }
                        //     }
                        // }
                        #endregion

                        #region 正常下载
                        if (localFolders[folder.BundleName].MD5 != folder.MD5)
                        {
                            if (FileManager.FileExist(LocalPath + folder.BundleName))
                            {
                                FileManager.DeleteFile(LocalPath + folder.BundleName);
                            }
                        }
                        #endregion
                    }
                }
                yield return new WaitForEndOfFrame();
                //下载manifest文件
                DownLoad(ServerUrl + folder.BundleName + ".manifest", (byte[] _manifest_data) =>
                {
                    if (FileManager.FileExist(LocalPath + folder.BundleName + ".manifest"))
                    {
                        FileManager.DeleteFile(LocalPath + folder.BundleName + ".manifest");
                    }
                    //将manifest文件写入本地
                    FileManager.CreateFile(LocalPath + folder.BundleName + ".manifest", _manifest_data);
                });
            }

            yield return new WaitForEndOfFrame();
            foreach (var folder in downloadFolders)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() => Finished);

                #region 断点续传
                // folder.Tag = "1";
                // UpdateLocalConfigTag(folder);
                // UpdateLocalConfigMD5(folder);
                // Finished = false;
                // UnityWebRequester requester = new UnityWebRequester(window);
                // requester.DownloadFile(ServerUrl + folder.BundleName, LocalPath + folder.BundleName, (size, progress) =>
                // {
                //     downloadingSize = (long)(size * progress);
                //     if (progress >= 1)
                //     {
                //         if (!Finished)
                //         {
                //             folder.Tag = "0";
                //             UpdateLocalConfigTag(folder);
                //             downloadingSize = 0;
                //             alreadyDownloadSize += Convert.ToInt64(folder.Size);
                //             Finished = true;
                //         }
                //     }
                // });
                #endregion
                
                #region 正常下载
                Finished = false;
                UnityWebRequester requester = new UnityWebRequester(window);
                requester.GetBytes(ServerUrl + folder.BundleName, (bytes) =>
                {
                    FileManager.CreateFile(LocalPath + folder.BundleName, bytes);
                    UpdateLocalConfigMD5(folder);
                    TimerManager.Instance.EndTimer(timeId);
                    downloadingSize = 0;
                    alreadyDownloadSize += Convert.ToInt64(folder.Size);
                    Finished = true;
                });
                timeId = TimerManager.Instance.StartTimer((time) =>
                {
                    downloadingSize = requester.DownloadedLength;
                });
                #endregion
            }
            yield return new WaitForEndOfFrame();
        }

        private void UpdateLocalConfigMD5(Folder folder)
        {
            InitLocalConfig();

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
                        }
                    }
                }
            }
            XmlManager.Save(xmlDocument, XmlLocalVersionPath);
        }

        private void UpdateLocalConfigTag(Folder folder)
        {
            InitLocalConfig();
            
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
                            folders[j].Attributes["Tag"].Value = folder.Tag;
                        }
                    }
                }
            }

            XmlManager.Save(xmlDocument, XmlLocalVersionPath);
        }
        private void InitLocalConfig()
        {
            if (!FileManager.FileExist(XmlLocalVersionPath))
            {
                AssetBundleConfig config = new AssetBundleConfig();
                config.GameVersion = serverABConfig.GameVersion;
                config.Platform = serverABConfig.Platform;
                config.Scenes = serverABConfig.Scenes;
                FileManager.CreateFile(XmlLocalVersionPath, XmlManager.ProtoByteSerialize<AssetBundleConfig>(config));
                
                XmlDocument xmlDocument = XmlManager.Load(XmlLocalVersionPath);

                var scene = xmlDocument.GetElementsByTagName("Scenes");
                for (int i = 0; i < scene.Count; i++)
                {
                    var folders = scene[i].ChildNodes;
                    for (int j = 0; j < folders.Count; j++)
                    {
                        folders[j].Attributes["Tag"].Value = "0";
                        folders[j].Attributes["MD5"].Value = "";
                    }
                }

                XmlManager.Save(xmlDocument, XmlLocalVersionPath);
            }
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
        private void CheckUpdate(AssetBundleConfig localABConfig, Action<UpdateMold> cb = null)
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
                    SetABScenePairs(serverABConfig);
                    Debug.Log($"大版本比较 v{Application.version}/v{serverABConfig.GameVersion}");
                    if (Application.version == serverABConfig.GameVersion)
                    {
                        serverFolders = GetFolders(serverABConfig);
                        if (localABConfig != null)
                        {
                            localFolders = GetFolders(localABConfig);
                            foreach (var serverfolder in serverFolders)
                            {
                                foreach (var localFoder in localFolders)
                                {
                                    if (serverfolder.Key == localFoder.Key)
                                    {
                                        if (serverfolder.Value.MD5 != localFoder.Value.MD5)
                                        {
                                            downloadFolders.Add(serverfolder.Value);
                                        }
                                        else
                                        {
                                            alreadlyFolders.Add(serverfolder.Value);
                                        }
                                    }
                                }

                                if (!localFolders.ContainsKey(serverfolder.Key))
                                {
                                    downloadFolders.Add(serverfolder.Value);
                                }
                            }
                        }
                        else
                        {
                            foreach (var serverfolder in serverFolders)
                            {
                                downloadFolders.Add(serverfolder.Value);
                            }
                        }
                        alreadyDownloadSize = GetSize(alreadlyFolders);
                        TotalSize = GetSize(serverFolders.Values.ToList()) / 1024f / 1024f;
                        UpdateMold mold = downloadFolders.Count > 0 ? UpdateMold.Hotfix : UpdateMold.None;
                        cb?.Invoke(mold);
                    }
                    else
                    {
                        // if (FileManager.FolderExist(LocalPath))
                        // {
                        //     FileManager.DeleteFolderAllFile(LocalPath);
                        // }
                        //大版本更新,下载新程序覆盖安装
                        Debug.Log("大版本更新,下载新程序覆盖安装");
                        cb?.Invoke(UpdateMold.App);
                    }
                });
            }
        }

        private long GetSize(List<Folder> Folders)
        {
            long sum = 0;
            for (int i = 0; i < Folders.Count; i++)
            {
                long s = Convert.ToInt64(Folders[i].Size);
                sum += s;
            }
            return sum;
        }

        private Dictionary<string, Folder> GetFolders(AssetBundleConfig config)
        {
            Dictionary<string, Folder> folders = new Dictionary<string, Folder>();
            for (int i = 0; i < config.Scenes.Count; i++)
            {
                for (int j = 0; j < config.Scenes[i].Folders.Count; j++)
                {
                    if (!folders.ContainsKey(config.Scenes[i].Folders[j].BundleName))
                    {
                        folders.Add(config.Scenes[i].Folders[j].BundleName, config.Scenes[i].Folders[j]);
                    }
                }
            }
            return folders;
        }

        /// <summary> 获取文件夹名和包名 </summary>
        private void SetABScenePairs(AssetBundleConfig config)
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