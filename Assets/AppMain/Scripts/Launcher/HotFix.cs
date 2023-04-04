using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AppFrame.Config;
using UnityEngine;
using UnityEngine.Networking;


namespace Launcher
{
    public class HotFix
    {
        private static string LocalPath;
        private static string LocalVersionConfigPath;

        private static string ServerUrl;
        private static string ServerVersionConfigPath;

        private static AssetBundleConfig localABConfig;
        private static AssetBundleConfig serverABConfig;

        private static Dictionary<string, Folder> localFolders = new Dictionary<string, Folder>();
        private static Dictionary<string, Folder> serverFolders = new Dictionary<string, Folder>();

        private static List<Folder> downloadFolders = new List<Folder>();
        private static List<Folder> alreadlyFolders = new List<Folder>();

        public static float TotalSize = 0; // 资源的总大小 MB
        private static long alreadyDownloadSize = 0;
        private static long downloadingSize;

        private static string server_url;

        public static void Init(Action<bool> callback)
        {
            LocalPath = $"{Application.persistentDataPath}/AssetBundle/Hybrid/{AssetBundleManager.PlatformName}/";
            ServerUrl = $"{server_url}/AssetBundle/Hybrid/{AssetBundleManager.PlatformName}/";

            LocalVersionConfigPath = LocalPath + "AssetBundleConfig.json";
            ServerVersionConfigPath = ServerUrl + "AssetBundleConfig.json";

            switch (Launcher.AppConfig.LoadAssetsMold)
            {
                case LoadAssetsMold.Native:
                    callback?.Invoke(false);
                    break;
                case LoadAssetsMold.Local:
                    var localPath = $"{Application.streamingAssetsPath}/AssetBundle/Hybrid/{AssetBundleManager.PlatformName}/";
                    var configPath = localPath + "AssetBundleConfig.json";
                    DownLoad(configPath, (string data) =>
                    {
                        var config = JsonUtility.FromJson<AssetBundleConfig>(data);
                        SetABModulePairs(config);
                        callback?.Invoke(true);
                    });
                    break;
                case LoadAssetsMold.Remote:
                    StartUpdate((isUpdate, des) =>
                    {
                        if (isUpdate)
                        {
                            HotFix.StartDownLoad(() =>
                            {
                                callback?.Invoke(isUpdate);
                            });
                        }
                        else
                        {
                            SetABModulePairs(localABConfig);
                            callback?.Invoke(isUpdate);
                        }
                    });
                    break;
            }
        }

        public static void StartUpdate(Action<bool, string> UpdateCallBack)
        {
            if (File.Exists(LocalVersionConfigPath))
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

        private static void CheckUpdate(AssetBundleConfig localABConfig, Action<bool> cb = null)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //提示网络错误，检测网络链接是否正常
            }
            else
            {
                DownLoad(ServerVersionConfigPath, (string data) =>
                {
                    serverABConfig = JsonUtility.FromJson<AssetBundleConfig>(data);
                    SetABModulePairs(serverABConfig);
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
                        var mold = downloadFolders.Count > 0;
                        cb?.Invoke(mold);
                    }
                    else
                    {
                        cb?.Invoke(false);
                    }
                });
            }
        }

        private static bool CheckVersion(string version)
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
                    if (num_server > num_local)
                    {
                        isUpdateApp = true;
                        break;
                    }
                }
            }

            return isUpdateApp;
        }

        private static long GetSize(List<Folder> Folders)
        {
            long sum = 0;
            for (int i = 0; i < Folders.Count; i++)
            {
                long s = Convert.ToInt64(Folders[i].Size);
                sum += s;
            }

            return sum;
        }

        private static Dictionary<string, Folder> GetFolders(AssetBundleConfig config)
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
        /// <summary> 获取下载大小(M) </summary>
        public static float GetLoadedSize
        {
            get
            {
                return (alreadyDownloadSize + downloadingSize) / 1024f / 1024f;
            }
        }
        /// <summary> 获取下载进度 </summary>
        public static float GetProgress
        {
            get
            {
                return GetLoadedSize / TotalSize;
            }
        }
        /// <summary> 下载AB包 </summary>
        public static void StartDownLoad(Action callback)
        {
            // 下载总manifest文件
            DownLoad(ServerUrl + AssetBundleManager.PlatformName + ".manifest", (byte[] manifest_data) =>
            {
                //将manifest文件写入本地
                FileTool.CreateFile(LocalPath + AssetBundleManager.PlatformName + ".manifest", manifest_data);
                if (Launcher.AppConfig.ABPipeline == ABPipeline.Default)
                {
                    //下载总AB包
                    DownLoad(ServerUrl + AssetBundleManager.PlatformName, (byte[] ab_data) =>
                    {
                        //将ab文件写入本地
                        FileTool.CreateFile(LocalPath + AssetBundleManager.PlatformName, ab_data);
                        callback?.Invoke();
                    });
                }
                else if (Launcher.AppConfig.ABPipeline == ABPipeline.Scriptable)
                {
                    callback?.Invoke();
                }
            });
        }

        private static bool Finished = true;
        public static IEnumerator DownLoadAssetBundle()
        {
            for (int i = 0; i < downloadFolders.Count; i++)
            {
                var folder = downloadFolders[i];
                if (localFolders.Count > 0)
                {
                    if (localFolders.ContainsKey(folder.BundleName))
                    {
                        if (localFolders[folder.BundleName].MD5 != folder.MD5)
                        {
                            if (FileTool.FileExist(LocalPath + folder.BundleName))
                            {
                                FileTool.DeleteFile(LocalPath + folder.BundleName);
                            }
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
                if (Launcher.AppConfig.ABPipeline == ABPipeline.Default)
                {
                    //下载manifest文件
                    DownLoad(ServerUrl + folder.BundleName + ".manifest", (byte[] _manifest_data) =>
                    {
                        if (FileTool.FileExist(LocalPath + folder.BundleName + ".manifest"))
                        {
                            FileTool.DeleteFile(LocalPath + folder.BundleName + ".manifest");
                        }
                        //将manifest文件写入本地
                        FileTool.CreateFile(LocalPath + folder.BundleName + ".manifest", _manifest_data);
                    });
                }
                
            }

            yield return new WaitForEndOfFrame();
            foreach (var folder in downloadFolders)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(() => Finished);
                Finished = false;
                DownLoad(ServerUrl + folder.BundleName, (byte[] bytes) =>
                {
                    FileTool.CreateFile(LocalPath + folder.BundleName, bytes);
                    UpdateLocalConfigMD5(folder);
                    downloadingSize = 0;
                    alreadyDownloadSize += Convert.ToInt64(folder.Size);
                    Finished = true;
                });
            }
            yield return new WaitForEndOfFrame();
        }

        private static AssetBundleConfig alreadyConfig = null;
        private static void UpdateLocalConfigMD5(Folder folder)
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
            if (FileTool.FileExist(LocalVersionConfigPath))
            {
                FileTool.DeleteFile(LocalVersionConfigPath);
            }

            string json = JsonUtility.ToJson(alreadyConfig, true);
            FileTool.CreateFile(LocalVersionConfigPath,json);
        }

        private static void UpdateLocalConfigTag(Folder folder)
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
            
            if (FileTool.FileExist(LocalVersionConfigPath))
            {
                FileTool.DeleteFile(LocalVersionConfigPath);
            }

            string json = JsonUtility.ToJson(alreadyConfig, true);
            FileTool.CreateFile(LocalVersionConfigPath,json);
        }
        private static void InitLocalConfig()
        {
            if (alreadyConfig == null)
            {
                alreadyConfig = new AssetBundleConfig();
                alreadyConfig.GameVersion = serverABConfig.GameVersion;
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
                        f.Mold = folder.Mold;
                        f.MD5 = "";
                        f.Size = folder.Size;
                        m.Folders.Add(f);
                    }
                    alreadyConfig.Modules.Add(m);
                }
                
                if (FileTool.FileExist(LocalVersionConfigPath))
                {
                    FileTool.DeleteFile(LocalVersionConfigPath);
                }

                string json = JsonUtility.ToJson(alreadyConfig, true);
                FileTool.CreateFile(LocalVersionConfigPath,json);
            }
        }
        /// <summary> 获取文件夹名和包名 </summary>
        private static void SetABModulePairs(AssetBundleConfig config)
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

        private static void DownLoad(string url, Action<byte[]> callback)
        {
            var unityWebRequest = new UnityWebRequest();
            unityWebRequest.url = url;
            unityWebRequest.method = UnityWebRequest.kHttpVerbGET;
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            var async = unityWebRequest.SendWebRequest();
            async.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(unityWebRequest.error))
                {
                    callback?.Invoke(unityWebRequest.downloadHandler.data);
                }
                else
                {
                    Debug.LogError($"[Error:Bytes] {url} {unityWebRequest.error}");
                }
            };
        }

        private static void DownLoad(string url, Action<string> callback)
        {
            var unityWebRequest = new UnityWebRequest();
            unityWebRequest.url = url;
            unityWebRequest.method = UnityWebRequest.kHttpVerbGET;
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            var async = unityWebRequest.SendWebRequest();
            async.completed += (ao) =>
            {
                if (string.IsNullOrEmpty(unityWebRequest.error))
                {
                    callback?.Invoke(unityWebRequest.downloadHandler.text);
                }
                else
                {
                    Debug.LogError($"[Error:Bytes] {url} {unityWebRequest.error}");
                }
            };
        }
    }
}