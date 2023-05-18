using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using App;
using AppFrame.Config;
using AppFrame.Data;
using AppFrame.Enum;
using AppFrame.Info;
using AppFrame.Interface;
using AppFrame.Manager;
using AppFrame.Tools;
using UnityEngine;

namespace Modules.Update
{
    public class UpdateLogic : SingletonEvent<UpdateLogic>, ILogic
    {
        private UpdateView view;

        #region Private Var

        private string LocalPath;
        private string LocalVersionConfigPath;

        private string ServerUrl;
        private string ServerVersionConfigPath;

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

        public UpdateLogic()
        {
            AddEventMsg("UpdateNow", () => { UpdateNow(); });//
        }

        public void Begin()
        {
            LocalPath = PlatformManager.Instance.GetDataPath($"AssetBundle/{PlatformManager.Instance.Name}/{Application.version}/{AppInfo.AppConfig.ResVersion}/Assets/");
            ServerUrl = NetcomManager.ABUrl + $"{PlatformManager.Instance.Name}/{Application.version}/{AppInfo.AppConfig.ResVersion}/Assets/";

            LocalVersionConfigPath = LocalPath + "AssetBundleConfig.json";
            ServerVersionConfigPath = ServerUrl + "AssetBundleConfig.json";

            appUrl = NetcomManager.AppUrl + "meta.apk";
            appPath = PlatformManager.Instance.GetDataPath("App/meta.apk");

            switch (AppInfo.AppConfig.LoadAssetsMold)
            {
                case LoadAssetsMold.Native:
                    Root.StartApp();
                    break;
                case LoadAssetsMold.Local:
                    var localPath = PlatformManager.Instance.GetAssetsPath($"AssetBundle/{PlatformManager.Instance.Name}/{Application.version}/{AppInfo.AppConfig.ResVersion}/Assets/");
                    var configPath = localPath + "AssetBundleConfig.json";
                    DownLoad(configPath, (string data) =>
                    {
                        var config = JsonUtility.FromJson<AssetBundleConfig>(data);
                        SetABModulePairs(config);
                        Root.StartApp();
                    });
                    break;
                case LoadAssetsMold.Remote:
                    view = AssetsManager.Instance.LoadUIView<UpdateView>(Launcher.Launcher.UpdateView);
                    view.SetViewActive();

                    view.SetTipsText("检查更新中...");
                    view.SetProgressValue(0);
                    StartUpdate((UpdateMold mold, string des) =>
                    {
                        UpdateMold = mold;
                        Log.I($"更新结果:{mold}");
                        switch (mold)
                        {
                            case UpdateMold.Hotfix:
                                view.SetContentText(des);
                                view.SetUpdateTipsActive(true);
                                break;
                            case UpdateMold.App:
                                view.SetContentText($"发现新版本应用:v{serverABConfig.GameVersion}");
                                view.SetUpdateTipsActive(true);
                                break;
                            case UpdateMold.None:
                                view.SetViewActive(false);
                                SetABModulePairs(localABConfig);
                                Root.StartApp();
                                break;
                        }
                    });
                    break;
            }
        }

        public void End()
        {
        }

        private void UpdateNow()
        {
            view.SetUpdateTipsActive(false);
            view.SetProgressBarActive(true);
            view.SetTipsText("下载中...");
            view.StartCoroutine(DownLoading());
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
                        view.SetSpeedText(speed);
                        view.SetProgressText(GetLoadedSize, TotalSize);
                        view.SetProgressValue(GetProgress);
                    }
                    yield return new WaitForEndOfFrame();
                    view.SetProgressText(TotalSize, TotalSize);
                    view.SetProgressValue(GetProgress);
                    yield return new WaitForSeconds(0.2f);
                    //更新下载完成，开始运行App
                    view.SetViewActive(false);
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
                    // UnityWebRequester requester = NetcomManager.Uwr;
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
                    if (FileTools.FileExist(appPath))
                    {
                        FileTools.DeleteFile(appPath);
                    }
                    UnityWebRequester requester = NetcomManager.Uwr;
                    requester.GetBytes(appUrl, (bytes) =>
                    {
                        FileTools.CreateFile(appPath, bytes);
                        TimeUpdateManager.Instance.EndTimer(timeId);
                        PlatformManager.Instance.InstallApp(appPath);
                    });
                    timeId = TimeUpdateManager.Instance.StartTimer((t) =>
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
                        view.SetSpeedText(speed);
                        view.SetProgressText(size, size / requester.DownloadedProgress);
                        view.SetProgressValue(requester.DownloadedProgress);
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
            if (FileTools.FileExist(LocalVersionConfigPath))
            {
                //读取本地热更文件
                DownLoad($"file://{LocalVersionConfigPath}", (string data) =>
                {
                    localABConfig = !string.IsNullOrEmpty(data)
                        ? JsonUtility.FromJson<AssetBundleConfig>(data)
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
                FileTools.CreateFile(LocalPath + PlatformManager.Instance.Name + ".manifest", manifest_data);
                if (AppInfo.AppConfig.ABPipeline == ABPipeline.Default)
                {
                     //下载总AB包
                     DownLoad(ServerUrl + PlatformManager.Instance.Name, (byte[] ab_data) =>
                     {
                        //将ab文件写入本地
                        FileTools.CreateFile(LocalPath + PlatformManager.Instance.Name, ab_data);
                        view.StartCoroutine(DownLoadAssetBundle());
                     });
                }
                else if(AppInfo.AppConfig.ABPipeline == ABPipeline.Scriptable)
                {
                    view.StartCoroutine(DownLoadAssetBundle());
                }
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
                            if (FileTools.FileExist(LocalPath + folder.BundleName))
                            {
                                FileTools.DeleteFile(LocalPath + folder.BundleName);
                            }
                        }
                        #endregion
                    }
                }
                yield return new WaitForEndOfFrame();
                if (AppInfo.AppConfig.ABPipeline == ABPipeline.Default)
                {
                    //下载manifest文件
                    DownLoad(ServerUrl + folder.BundleName + ".manifest", (byte[] _manifest_data) =>
                    {
                        if (FileTools.FileExist(LocalPath + folder.BundleName + ".manifest"))
                        {
                            FileTools.DeleteFile(LocalPath + folder.BundleName + ".manifest");
                        }
                        //将manifest文件写入本地
                        FileTools.CreateFile(LocalPath + folder.BundleName + ".manifest", _manifest_data);
                    });
                }
                
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
                // UnityWebRequester requester = NetcomManager.Uwr;
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
                UnityWebRequester requester = NetcomManager.Uwr;
                requester.GetBytes(ServerUrl + folder.BundleName, (bytes) =>
                {
                    FileTools.CreateFile(LocalPath + folder.BundleName, bytes);
                    UpdateLocalConfigMD5(folder);
                    TimeUpdateManager.Instance.EndTimer(timeId);
                    downloadingSize = 0;
                    alreadyDownloadSize += Convert.ToInt64(folder.Size);
                    Finished = true;
                });
                timeId = TimeUpdateManager.Instance.StartTimer((time) =>
                {
                    downloadingSize = requester.DownloadedLength;
                });
                #endregion
            }
            yield return new WaitForEndOfFrame();
        }

        private AssetBundleConfig alreadyConfig = null;
        private void UpdateLocalConfigMD5(Folder folder)
        {
            InitLocalConfig();

            for (int i = 0; i < alreadyConfig.Modules.Count; i++)
            {
                var folders = alreadyConfig.Modules[i].Folders;
                for (int j = 0; j < folders.Count; j++)
                {
                    if (folder != null)
                    {
                        if (folder.BundleName == folders[j].BundleName)
                        {
                            folders[j].MD5 = folder.MD5;
                        }
                    }
                }
            }
            if (FileTools.FileExist(LocalVersionConfigPath))
            {
                FileTools.DeleteFile(LocalVersionConfigPath);
            }

            string json = JsonUtility.ToJson(alreadyConfig, true);
            FileTools.CreateFile(LocalVersionConfigPath,json);
        }

        private void UpdateLocalConfigTag(Folder folder)
        {
            InitLocalConfig();
            
            for (int i = 0; i < alreadyConfig.Modules.Count; i++)
            {
                var folders = alreadyConfig.Modules[i].Folders;
                for (int j = 0; j < folders.Count; j++)
                {
                    if (folder != null)
                    {
                        if (folder.BundleName == folders[j].BundleName)
                        {
                            folders[j].Tag = folder.Tag;
                        }
                    }
                }
            }
            
            if (FileTools.FileExist(LocalVersionConfigPath))
            {
                FileTools.DeleteFile(LocalVersionConfigPath);
            }

            string json = JsonUtility.ToJson(alreadyConfig, true);
            FileTools.CreateFile(LocalVersionConfigPath,json);
        }
        private void InitLocalConfig()
        {
            if (alreadyConfig == null)
            {
                alreadyConfig = new AssetBundleConfig();
                alreadyConfig.GameVersion = serverABConfig.GameVersion;
                alreadyConfig.ResVersion = serverABConfig.ResVersion;
                alreadyConfig.Platform = serverABConfig.Platform;
                alreadyConfig.Des = serverABConfig.Des;
                alreadyConfig.Modules = new List<Module>();

                foreach (var module in serverABConfig.Modules)
                {
                    Module m = new Module();
                    m.ModuleName = module.ModuleName;
                    m.Folders = new List<Folder>();
                    foreach (var folder in module.Folders)
                    {
                        Folder f = new Folder();
                        f.FolderName = folder.FolderName;
                        f.BundleName = folder.BundleName;
                        f.Tag = "0";
                        f.MD5 = "";
                        f.Size = folder.Size;
                        m.Folders.Add(f);
                    }
                    alreadyConfig.Modules.Add(m);
                }
                
                if (FileTools.FileExist(LocalVersionConfigPath))
                {
                    FileTools.DeleteFile(LocalVersionConfigPath);
                }

                string json = JsonUtility.ToJson(alreadyConfig, true);
                FileTools.CreateFile(LocalVersionConfigPath,json);
            }
        }
        /// <summary> 下载 </summary>
        private void DownLoad(string url, Action<byte[]> action)
        {
            UnityWebRequester requester = NetcomManager.Uwr;
            requester.GetBytes(url, (byte[] data) =>
            {
                action?.Invoke(data);
                requester.Destory();
            });
        }
        /// <summary> 下载 </summary>
        private void DownLoad(string url, Action<string> action)
        {
            UnityWebRequester requester = NetcomManager.Uwr;
            requester.Get(url, (string data) =>
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
                Log.E("网络异常");
            }
            else
            {
                DownLoad(ServerVersionConfigPath, (string data) =>
                {
                    serverABConfig = JsonUtility.FromJson<AssetBundleConfig>(data);
                    SetABModulePairs(serverABConfig);
                    Log.I($"大版本比较 v{Application.version}/v{serverABConfig.GameVersion}");
                    if (!CheckVersion(serverABConfig.GameVersion))
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
                        Log.I("大版本更新,下载新程序覆盖安装");
                        cb?.Invoke(UpdateMold.App);
                    }
                });
            }
        }
        
        private bool CheckVersion(string version)
        {
            bool isUpdateApp = false;
            string[] local = Application.version.Split('.');
            string[] server = version.Split('.');
            if (local.Length == server.Length)
            {
                for (int i = 0; i < local.Length; i++)
                {
                    int num_local = int.Parse(local[i]);
                    int num_server = int.Parse(server[i]);
                    Log.I($"{num_local}-{num_server}");
                    if (num_server > num_local)
                    {
                        isUpdateApp = true;
                        break;
                    }
                }
            }
            return isUpdateApp;
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
            for (int i = 0; i < config.Modules.Count; i++)
            {
                for (int j = 0; j < config.Modules[i].Folders.Count; j++)
                {
                    if (!folders.ContainsKey(config.Modules[i].Folders[j].BundleName))
                    {
                        folders.Add(config.Modules[i].Folders[j].BundleName, config.Modules[i].Folders[j]);
                    }
                }
            }
            return folders;
        }

        /// <summary> 获取文件夹名和包名 </summary>
        private void SetABModulePairs(AssetBundleConfig config)
        {
            //获取文件夹名和包名，用来给AssetbundleSceneManager里的folderDic赋值
            foreach (var module in config.Modules)
            {
                Dictionary<string, Folder> folderPairs = new Dictionary<string, Folder>();
                foreach (var folder in module.Folders)
                {
                    if (!folderPairs.ContainsKey(folder.FolderName))
                    {
                        folderPairs.Add(folder.FolderName, folder);
                    }
                }

                AssetBundleManager.Instance.SetAbModulePairs(module.ModuleName, folderPairs);
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