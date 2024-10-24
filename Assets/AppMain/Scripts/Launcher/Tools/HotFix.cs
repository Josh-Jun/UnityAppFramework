using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static HybridABConfig localABConfig;
        private static HybridABConfig serverABConfig;

        private static Dictionary<string, Folder> localFolders = new Dictionary<string, Folder>();
        private static Dictionary<string, Folder> serverFolders = new Dictionary<string, Folder>();

        private static List<Folder> downloadFolders = new List<Folder>();
        private static List<Folder> alreadlyFolders = new List<Folder>();

        public static float TotalSize = 0; // 资源的总大小 MB
        private static long alreadyDownloadSize = 0;
        private static long downloadingSize;
        
        private static string ABUrl
        {
            get
            {
                string test_url = Application.dataPath.Replace("/Assets", ""); //本地AB包地址
                string pro_url = "https://meta-oss.genimous.com/vr-ota/dev_test/AssetBundle/"; //服务器AB包地址
                return Global.AppConfig.IsTestServer ? test_url : pro_url;
            }
        }

        public static void Init(Action<bool> callback)
        {
            LocalPath = $"{Application.persistentDataPath}/AssetBundle/{HybridABManager.PlatformName}/{Application.version}/{Global.AppConfig.ResVersion}/Hybrid/";
            ServerUrl = $"{ABUrl}/AssetBundle/{HybridABManager.PlatformName}/{Application.version}/{Global.AppConfig.ResVersion}/Hybrid/";

            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                ServerUrl = $"file://{ServerUrl}";
            }
            
            LocalVersionConfigPath = LocalPath + "AssetBundleConfig.json";
            ServerVersionConfigPath = ServerUrl + "AssetBundleConfig.json";
            switch (Global.AppConfig.LoadAssetsMold)
            {
                case LoadAssetsMold.Native:
                    callback?.Invoke(false);
                    break;
                case LoadAssetsMold.Local:
                    var localPath = $"{Application.streamingAssetsPath}/AssetBundle/{HybridABManager.PlatformName}/{Application.version}/{Global.AppConfig.ResVersion}/Hybrid/";
                    var configPath = localPath + "AssetBundleConfig.json";
                    if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        configPath = $"file://{configPath}";
                    }
                    DownLoad(configPath, (string data) =>
                    {
                        var config = JsonUtility.FromJson<HybridABConfig>(data);
                        SetABModulePairs(config);
                        callback?.Invoke(true);
                    });
                    break;
                case LoadAssetsMold.Remote:
                    StartUpdate((isUpdate, des) =>
                    {
                        if (isUpdate)
                        {
                            StartDownLoad(() =>
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
                        ? JsonUtility.FromJson<HybridABConfig>(data)
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

        private static void CheckUpdate(HybridABConfig localABConfig, Action<bool> cb = null)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //提示网络错误，检测网络链接是否正常
            }
            else
            {
                DownLoad(ServerVersionConfigPath, (string data) =>
                {
                    serverABConfig = JsonUtility.FromJson<HybridABConfig>(data);
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

        private static Dictionary<string, Folder> GetFolders(HybridABConfig config)
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
            DownLoad(ServerUrl + HybridABManager.PlatformName + ".manifest", (byte[] manifest_data) =>
            {
                //将manifest文件写入本地
                HotfixTool.CreateFile(LocalPath + HybridABManager.PlatformName + ".manifest", manifest_data);
                if (Global.AppConfig.ABPipeline == ABPipeline.Default)
                {
                    //下载总AB包
                    DownLoad(ServerUrl + HybridABManager.PlatformName, (byte[] ab_data) =>
                    {
                        //将ab文件写入本地
                        HotfixTool.CreateFile(LocalPath + HybridABManager.PlatformName, ab_data);
                        callback?.Invoke();
                    });
                }
                else if (Global.AppConfig.ABPipeline == ABPipeline.Scriptable)
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
                            if (HotfixTool.FileExist(LocalPath + folder.BundleName))
                            {
                                HotfixTool.DeleteFile(LocalPath + folder.BundleName);
                            }
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
                if (Global.AppConfig.ABPipeline == ABPipeline.Default)
                {
                    //下载manifest文件
                    DownLoad(ServerUrl + folder.BundleName + ".manifest", (byte[] _manifest_data) =>
                    {
                        if (HotfixTool.FileExist(LocalPath + folder.BundleName + ".manifest"))
                        {
                            HotfixTool.DeleteFile(LocalPath + folder.BundleName + ".manifest");
                        }
                        //将manifest文件写入本地
                        HotfixTool.CreateFile(LocalPath + folder.BundleName + ".manifest", _manifest_data);
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
                    HotfixTool.CreateFile(LocalPath + folder.BundleName, bytes);
                    UpdateLocalConfigMD5(folder);
                    downloadingSize = 0;
                    alreadyDownloadSize += Convert.ToInt64(folder.Size);
                    Finished = true;
                });
            }
            yield return new WaitForEndOfFrame();
        }

        private static HybridABConfig alreadyConfig = null;
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
            if (HotfixTool.FileExist(LocalVersionConfigPath))
            {
                HotfixTool.DeleteFile(LocalVersionConfigPath);
            }

            string json = JsonUtility.ToJson(alreadyConfig, true);
            HotfixTool.CreateFile(LocalVersionConfigPath,json);
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
            
            if (HotfixTool.FileExist(LocalVersionConfigPath))
            {
                HotfixTool.DeleteFile(LocalVersionConfigPath);
            }

            string json = JsonUtility.ToJson(alreadyConfig, true);
            HotfixTool.CreateFile(LocalVersionConfigPath,json);
        }
        private static void InitLocalConfig()
        {
            if (alreadyConfig == null)
            {
                alreadyConfig = new HybridABConfig();
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
                
                if (HotfixTool.FileExist(LocalVersionConfigPath))
                {
                    HotfixTool.DeleteFile(LocalVersionConfigPath);
                }

                string json = JsonUtility.ToJson(alreadyConfig, true);
                HotfixTool.CreateFile(LocalVersionConfigPath,json);
            }
        }
        /// <summary> 获取文件夹名和包名 </summary>
        private static void SetABModulePairs(HybridABConfig config)
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

                HybridABManager.Instance.SetAbModulePairs(module.ModuleName, folderPairs);
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