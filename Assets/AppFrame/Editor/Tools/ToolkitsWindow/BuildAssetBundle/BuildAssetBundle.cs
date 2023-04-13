using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AppFrame.Config;
using AppFrame.Enum;
using AppFrame.Tools;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Build.Pipeline;

namespace AppFrame.Editor
{
    public class BuildAssetBundle
    {
        public static string outputPath;
        public static string buildPath;

        private static Dictionary<string, Dictionary<string, string>> moduleDic =
            new Dictionary<string, Dictionary<string, string>>();

        public static string des = "请输入本版更新描述";

        private static AppConfig AppConfig; //App配置表 
        private static string configPath = "AppConfig";

        private static string m_TmpBuildPath = "";

        private static ABMold mold = ABMold.Hybrid;

        public static void Init()
        {
            AppConfig = Resources.Load<AppConfig>(configPath);

            buildPath = "Assets/Resources/AssetsFolder";
            outputPath = Application.dataPath.Replace("Assets", "AssetBundle");
        }

        public static void SetBuildPath(ABMold mold)
        {
            BuildAssetBundle.mold = mold;
            buildPath = mold == ABMold.Hybrid
                ? buildPath.Replace("AssetsFolder", "HybridFolder")
                : buildPath.Replace("HybridFolder", "AssetsFolder");
            GetFolders_Layer1();
        }

        public static void SelectList(bool value)
        {
            for (int i = 0; i < m_ExportList.Count; i++)
                m_ExportList[i] = value;
        }

        public static void SetResVersion(string ResVersion)
        {
            AppConfig.ResVersion = ResVersion;
            EditorUtility.SetDirty(AppConfig);
        }
        public static string GetResVersion()
        {
            return AppConfig.ResVersion;
        }

        public static void AutoBuildAssetBundle(BuildTarget buildTarget)
        {
            if (mold == ABMold.Hybrid)
            {
                BuildAndCopyDll(buildTarget);
            }
            RemoveAllAssetBundleLabels();
            SetAssetBundleLabels();
            DeleteAssetBundle(buildTarget);
            BuildAllAssetBundles(buildTarget);
            CreateFile(buildTarget);
        }

        public static void BuildAndCopyDll(BuildTarget buildTarget)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            PrebuildCommand.GenerateAll();
            CopyDLLToSourceData(buildTarget);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }


        private static void CopyDLLToSourceData(BuildTarget target)
        {
            string targetDstDir = $"{Application.dataPath}/Resources/HybridFolder/App/Dll";
            CopyAOTAssembliesToSourceData(target, targetDstDir);
            CopyMyAssembliesToSourceData(target, targetDstDir);
        }
        private static void CopyAOTAssembliesToSourceData(BuildTarget target, string targetDstDir)
        {
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            Directory.CreateDirectory(targetDstDir);

            List<string> list = new List<string>();
            list.AddRange(Launcher.Launcher.AOTMetaAssemblyNames);
            foreach (var dll in list)
            {
                string srcDllPath = $"{aotAssembliesSrcDir}/{dll}";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError(
                        $"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }

                string dllBytesPath = $"{targetDstDir}/{dll}.bytes";

                Debug.Log($"[CopyAOTAssembliesToSourceData] copy AOT dll {srcDllPath} -> {dllBytesPath}");
                File.Copy(srcDllPath, dllBytesPath, true);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void CopyMyAssembliesToSourceData(BuildTarget target, string targetDstDir)
        {
            string aotAssembliesSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            Directory.CreateDirectory(targetDstDir);

            List<string> list = new List<string>();
            list.AddRange(Launcher.Launcher.HotfixAssemblyNames);
            foreach (var dll in list)
            {
                string srcDllPath = $"{aotAssembliesSrcDir}/{dll}";
                if (!File.Exists(srcDllPath))
                {
                    Debug.LogError(
                        $"ab中添加AOT补充元数据dll:{srcDllPath} 时发生错误,文件不存在。裁剪后的AOT dll在BuildPlayer时才能生成，因此需要你先构建一次游戏App后再打包。");
                    continue;
                }

                string dllBytesPath = $"{targetDstDir}/{dll}.bytes";

                Debug.Log($"[CopyMyAssembliesToSourceData] copy AOT dll {srcDllPath} -> {dllBytesPath}");
                File.Copy(srcDllPath, dllBytesPath, true);
            }
        }

        #region 自动做标记

        public static void RemoveAllAssetBundleLabels()
        {
            moduleDic.Clear();
            var names = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in names)
            {
                AssetDatabase.GetAssetPathsFromAssetBundle(name).Select(AssetImporter.GetAtPath).ToList()
                    .ForEach(x => x.assetBundleName = string.Empty);
            }
            //AssetDatabase.Refresh();
        }

        //[MenuItem("AssetBundle/Set AssetBundle Labels(自动做标记)")]
        public static void SetAssetBundleLabels()
        {
            Init_SelectFolderDic();

            // 移除没用到的包名
            AssetDatabase.RemoveUnusedAssetBundleNames();

            string assetDirectory = Application.dataPath.Replace("Assets", "") + buildPath;

            DirectoryInfo directoryInfo = new DirectoryInfo(assetDirectory);
            // 获取AssetsFolder下的所有子文件夹
            DirectoryInfo[] scenendirectories = directoryInfo.GetDirectories();
            // 2. 遍历里面的每个子文件夹里的的文件夹
            foreach (DirectoryInfo tempDirectoryInfo in scenendirectories)
            {
                // 获取每个子文件夹下的文件夹
                string sceneDirectory = assetDirectory + "/" + tempDirectoryInfo.Name;
                DirectoryInfo sceneDirectoryInfo = new DirectoryInfo(sceneDirectory);
                if (sceneDirectoryInfo == null)
                {
                    Debug.LogError("场景文件夹" + sceneDirectory + "不存在");
                    Debug.LogError("该目录下不存在此文件夹!" + sceneDirectory);
                    return;
                }
                else
                {
                    // 7. 保存对应的文件夹名和具体路径 ；
                    Dictionary<string, string> namePathDic = new Dictionary<string, string>();

                    //  3. 遍历场景文件夹里的所有文件系统             
                    int index = sceneDirectory.LastIndexOf('/');
                    string sceneName = sceneDirectory.Substring(index + 1);

                    if (m_SelectFolderDic.ContainsKey(sceneName))
                        OnSceneFileSystemInfo(sceneDirectoryInfo, sceneName, namePathDic);

                    //OnWriteConfig(sceneName, namePathDic);
                    if (!moduleDic.ContainsKey(tempDirectoryInfo.Name))
                        moduleDic.Add(tempDirectoryInfo.Name, namePathDic);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("AssetBundle Labels设置成功!");
        }

        /// <summary>
        /// 3. 遍历场景文件夹里的所有文件系统
        /// </summary>
        /// <param name="fileSystemInfo">文件</param>
        /// <param name="sceneName">场景名字</param>
        public static void OnSceneFileSystemInfo(FileSystemInfo fileSystemInfo, string sceneName,
            Dictionary<string, string> namePathDic)
        {
            if (!fileSystemInfo.Exists)
                Debug.LogError(fileSystemInfo.FullName + "不存在！");
            string name = AppConfig.TargetPackage == TargetPackage.Mobile ? "Pico" : "Mobile";
            DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
            // 获取所有文件系统[包括文件夹和文件]
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
            foreach (FileSystemInfo tempfileSystemInfo in fileSystemInfos)
            {
                if (tempfileSystemInfo.Name != name)
                {
                    FileInfo fileInfo = tempfileSystemInfo as FileInfo;
                    if (fileInfo == null)
                    {
                        // 代表强转失败 ,不是文件 , 是文件夹
                        // 4. 如果访问的是文件夹 ， 再继续访问里面的所有文件系统，直到找到文件 (递归思想)；
                        // 再调用本方法 
                        OnSceneFileSystemInfo(tempfileSystemInfo, sceneName, namePathDic);
                    }
                    else
                    {
                        // 代表强转成功了 , 是文件 ;
                        // 5. 找到文件 ， 就要修改它的 AssetBundle Labels ;
                        SetLabels(fileInfo, sceneName, namePathDic);
                    }
                }
            }
        }

        /// <summary>
        /// 5. 找到文件 ，就要修改它的 AssetBundle Labels ;
        /// </summary>
        public static void SetLabels(FileInfo fileInfo, string sceneName, Dictionary<string, string> namePathDic)
        {
            // Unity自身产生的 .meta文件 不用去读 ；
            if (fileInfo.Extension == ".meta")
                return;
            string bundleName = GetBundleName(fileInfo, sceneName);
            //Debug.Log("包名为 :" + bundleName );

            // D:\Unity_forWork\Unity_Project\AssetBundle02_Senior\Assets\AssetsFolder\Scene1\Character\NB\Player4.prefab
            // 获取Assetsy以及之后的目录
            int assetIndex = fileInfo.FullName.IndexOf("Assets");
            string assetPath = fileInfo.FullName.Substring(assetIndex);
            // Debug.Log(assetPath); // Assets\AssetsFolder\Scene1\Character\NB\Player4.prefab
            // 6.用 AssetImporter 类 ， 修改名称和后缀 ；
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath); // GetAtPath方法是获取Assets文件和之后的目录
            assetImporter.assetBundleName = bundleName.ToLower();
            // if (fileInfo.Extension == ".unity")
            //     assetImporter.assetBundleVariant = "sc";
            // else
            //     assetImporter.assetBundleVariant = "ab";

            // 添加到字典里 
            string folderName = "";
            if (bundleName.Contains("/"))
                folderName = bundleName.Split('/')[1]; // key
            else
                folderName = bundleName;
            // string bundlePath = assetImporter.assetBundleName; // value
            string bundlePath = assetImporter.assetBundleName /*+ "." + assetImporter.assetBundleVariant*/; // value
            if (!namePathDic.ContainsKey(folderName))
                namePathDic.Add(folderName, bundlePath);
        }

        /// <summary>
        /// 获取包名
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="sceneName"></param>
        public static string GetBundleName(FileInfo fileInfo, string sceneName)
        {
            var bundleName = "";
            // windows 全路径 \ 
            string windowsPath = fileInfo.FullName;
            // D:\Unity_forWork\Unity_Project\AssetBundle02_Senior\Assets\AssetsFolder\Scene1\Character\NB\Player4.prefab
            // Debug.Log(windowsPath);
            // 转换成Unity可识别的路径 把 \ 改为 /   [加@有奇效]     
            string unityPath = windowsPath.Replace(@"\", "/");
            // D:/Unity_forWork/Unity_Project/AssetBundle02_Senior/Assets/AssetsFolder/Scene1/Character/NB/Player4.prefab
            // D:/Unity_forWork/Unity_Project/AssetBundle02_Senior/Assets/AssetsFolder/Scene1/Scene1.unity
            // Debug.Log(unityPath);

            // 获取Scene之后的部分 :  Character/NB/Player4.prefab
            int sceneIndex = unityPath.IndexOf(sceneName) + sceneName.Length;
            string bundlePath = unityPath.Substring(sceneIndex + 1);
            
            if (bundlePath.Contains("/"))
            {
                // 后续还有路径, 包含子目录[取第一个/后的名字]
                // Character/NB/Player4.prefab
                string tempPath = bundlePath.Split('/')[0];
                bundleName = sceneName + "/" + tempPath;
            }
            else
            {
                // 如果是场景
                // Scene1.unity
                bundleName = sceneName;
            }

            if (sceneName == "Scene")
            {
                if (fileInfo.Extension == ".unity")
                {
                    bundleName += "Scene";
                }
                else
                {
                    bundleName += "Asset";
                }
            }

            return bundleName;
        }

        #endregion

        #region 打包
        
        //[MenuItem("AssetBundle/BuildAllAssetBundle(一键打包)")]
        public static void BuildAllAssetBundles(BuildTarget buildTarget)
        {
            string outPath = $"{outputPath}/{Application.version}/{AppConfig.ResVersion}/{mold}/{buildTarget}";
            if (!FileTools.FolderExist(outPath))
            {
                FileTools.CreateFolder(outPath);
            }

            if (AppConfig.ABPipeline == ABPipeline.Default)
            {
                AssetBundleManifest = BuildPipeline.BuildAssetBundles(outPath,
                    BuildAssetBundleOptions.ChunkBasedCompression,
                    buildTarget);
                if (AssetBundleManifest)
                {
                    AssetDatabase.Refresh();
                    EditorUtility.RevealInFinder(outPath);
                    Debug.Log("AssetBundle 打包成功! " + outPath);
                }
            }
            else
            {
                ScriptableAssetBundleManifest = ScriptableBuildAssetBundles(outPath, false, true, buildTarget);
                if (ScriptableAssetBundleManifest)
                {
                    AssetDatabase.Refresh();
                    EditorUtility.RevealInFinder(outPath);
                    Debug.Log("AssetBundle 打包成功! " + outPath);
                }
            }
        }

        public static CompatibilityAssetBundleManifest ScriptableBuildAssetBundles(string outputPath, bool forceRebuild,
            bool useChunkBasedCompression,
            BuildTarget buildTarget)
        {
            var options = BuildAssetBundleOptions.None;
            if (useChunkBasedCompression)
                options |= BuildAssetBundleOptions.ChunkBasedCompression;

            if (forceRebuild)
                options |= BuildAssetBundleOptions.ForceRebuildAssetBundle;

            // Get the set of bundle to build
            var bundles = ContentBuildInterface.GenerateAssetBundleBuilds();
            // Update the addressableNames to load by the file name without extension
            for (var i = 0; i < bundles.Length; i++)
                bundles[i].addressableNames = bundles[i].assetNames.Select(Path.GetFileNameWithoutExtension).ToArray();

            var manifest = CompatibilityBuildPipeline.BuildAssetBundles(outputPath, bundles, options, buildTarget);
            return manifest;
        }

        #endregion

        #region 删除

        //[MenuItem("AssetBundle/DeleteAllAssetBundle(一键删除)")]
        public static void DeleteAssetBundle(BuildTarget buildTarget)
        {
            string outPath = $"{outputPath}/{Application.version}/{AppConfig.ResVersion}/{mold}/{buildTarget}";
            if (!FileTools.FolderExist(outPath))
            {
                return;
            }

            Directory.Delete(outPath, true); // 删除 AssetBundle文件夹 , true 强势删除 ;
            File.Delete(outPath + ".meta"); // unity 自带的 .meta 文件也删掉[不删会报警告] ;
            AssetDatabase.Refresh();
        }

        #endregion

        #region 生成MD5文件

        private static CompatibilityAssetBundleManifest ScriptableAssetBundleManifest = null;
        private static AssetBundleManifest AssetBundleManifest = null;
        //[MenuItem("AssetBundle/CreateMD5File(生成MD5文件)")]
        public static void CreateFile(BuildTarget buildTarget)
        {
            string outPath = $"{outputPath}/{Application.version}/{AppConfig.ResVersion}/{mold}/{buildTarget}";
            string filePath = outPath + "/AssetBundleConfig.json";

            AssetBundleConfig config = new AssetBundleConfig();
            config.GameVersion = Application.version;
            config.Platform = buildTarget.ToString();
            config.Des = des;
            config.Modules = new List<Module>();
            foreach (var module in moduleDic)
            {
                Module m = new Module();
                m.ModuleName = module.Key;
                m.Folders = new List<Folder>();
                foreach (var folder in module.Value)
                {
                    FileInfo file = new FileInfo(outPath + "/" + folder.Value);
                    Folder f = new Folder();
                    f.FolderName = folder.Key;
                    f.BundleName = folder.Value;
                    f.Tag = "0";
                    f.Mold = ((int)AppConfig.ABPipeline).ToString();
                    f.MD5 = GetFileMD5(file.FullName);
                    f.Size = $"{file.Length}";
                    f.Dependencies = AppConfig.ABPipeline == ABPipeline.Default
                        ? AssetBundleManifest.GetAllDependencies(folder.Value)
                        : ScriptableAssetBundleManifest.GetAllDependencies(folder.Value);
                    m.Folders.Add(f);
                }

                config.Modules.Add(m);
            }

            var json = JsonUtility.ToJson(config, true);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.WriteAllText(filePath, json);

            AssetDatabase.Refresh();
            Debug.Log("MD5文件生成完毕!");
        }

        /// <summary>
        /// 遍历文件夹下所有的文件
        /// </summary>
        /// <param name="fileSystemInfo"></param>
        /// <param name="list"></param>
        public static void ListFiles(FileSystemInfo fileSystemInfo, ref List<string> list)
        {
            if (fileSystemInfo.Extension == ".meta")
                return;

            DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
            FileSystemInfo[] fileSystemInfoArr = directoryInfo.GetFileSystemInfos();

            foreach (FileSystemInfo item in fileSystemInfoArr)
            {
                // fileInfoItem != null 就是文件,就把该文件加到list里
                if (item is FileInfo fileInfoItem)
                {
                    list.Add(fileInfoItem.FullName.Replace("\\", "/"));
                }
                else // fileInfoItem == null 就是文件夹, 递归调用自己,再从该文件夹里遍历所有文件
                {
                    ListFiles(item, ref list);
                }
            }
        }

        /// <summary>
        /// 遍历文件夹下所有的文件
        /// </summary>
        /// <param name="fileSystemInfo"></param>
        /// <param name="list"></param>
        public static void ListFiles(FileSystemInfo fileSystemInfo, ref List<FileInfo> list)
        {
            if (fileSystemInfo.Extension == ".meta")
                return;
            DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
            FileSystemInfo[] fileSystemInfoArr = directoryInfo.GetFileSystemInfos();

            foreach (FileSystemInfo item in fileSystemInfoArr)
            {
                // fileInfoItem != null 就是文件,就把该文件加到list里
                if (item is FileInfo fileInfoItem)
                {
                    list.Add(fileInfoItem);
                }
                else // fileInfoItem == null 就是文件夹, 递归调用自己,再从该文件夹里遍历所有文件
                {
                    ListFiles(item, ref list);
                }
            }
        }

        /// <summary>
        /// 获取文件 MD5 值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileMD5(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);

            // 引入命名空间   using System.Security.Cryptography;
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] bt = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bt.Length; i++)
            {
                sb.Append(bt[i].ToString("x2"));
            }

            md5.Dispose();
            return sb.ToString();
        }

        #endregion

        public static List<bool> m_ExportList = new List<bool>();
        public static List<string> m_DataList = new List<string>();
        public static Dictionary<string, bool> m_SelectFolderDic = new Dictionary<string, bool>();

        public static void Init_SelectFolderDic()
        {
            m_SelectFolderDic.Clear();
            for (int i = 0; i < m_ExportList.Count; i++)
            {
                if (m_ExportList[i] && !m_SelectFolderDic.ContainsKey(m_DataList[i]))
                    m_SelectFolderDic.Add(m_DataList[i], true);
            }
        }

        public static void GetFolders_Layer1()
        {
            if (m_TmpBuildPath == "" || m_TmpBuildPath != buildPath)
            {
                m_TmpBuildPath = buildPath;
                string targetFolder = Application.dataPath.Replace("Assets", m_TmpBuildPath + "\\");

                m_DataList.Clear();
                m_ExportList.Clear();
                foreach (string path in Directory.GetDirectories(targetFolder))
                {
                    m_DataList.Add(path.Replace(targetFolder, ""));
                    m_ExportList.Add(true);
                }
            }
        }

        public static int SelectCount()
        {
            int selectCount = 0;
            for (int i = 0; i < m_ExportList.Count; i++)
            {
                if (m_ExportList[i])
                    selectCount++;
            }

            return selectCount;
        }
    }
}