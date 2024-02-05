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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Build.Pipeline;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public class BuildAssetBundle : IToolkitEditor
    {
        private string outputPath;
        private string buildPath;

        private Dictionary<string, Dictionary<string, string>> moduleDic =
            new Dictionary<string, Dictionary<string, string>>();

        private string des = "请输入本版更新描述";

        private AppConfig AppConfig; //App配置表 
        private string configPath = "AppConfig";

        private string m_TmpBuildPath = "";

        private ABMold mold = ABMold.Hybrid;

        public void OnCreate(VisualElement root)
        {
            Init();
            var build_target = root.Q<EnumField>("BuildTarget");
            build_target.Init(BuildTarget.Android);

            var build_path = root.Q<TextField>("BuildPath");
            build_path.value = "Assets/Resources/HybridFolder";
            root.Q<Button>("BuildPath_Browse").clicked += () =>
            {
                build_path.value = EditorTool.Browse();
                buildPath = build_path.value;
            };
            var output_path = root.Q<TextField>("OutputPath");
            output_path.value = Application.dataPath.Replace("Assets", "AssetBundle");
            root.Q<Button>("OutputPath_Browse").clicked += () =>
            {
                output_path.value = EditorTool.Browse(true);
                outputPath = output_path.value;
            };
            var res_version = root.Q<TextField>("ResVersion");
            res_version.value = AppConfig.ResVersion;
            res_version.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                AppConfig.ResVersion = evt.newValue;
                EditorUtility.SetDirty(AppConfig);
            });
            var update_des = root.Q<TextField>("UpdateDes");
            update_des.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                des = evt.newValue;
            });

            var folder_list = root.Q<ScrollView>("FolderList");
            var label_page = root.Q<Label>("ListText");

            RefreshAssetBundleList(folder_list, label_page);

            var ab_mold = root.Q<EnumField>("ABMold");
            ab_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (ABMold)System.Enum.Parse(typeof(ABMold), evt.newValue);
                SetBuildPath(mold);
                build_path.value = buildPath;
                RefreshAssetBundleList(folder_list, label_page);
                root.Q<Button>("BuildAndCopyDll").style.display =
                    mold == ABMold.Hybrid ? DisplayStyle.Flex : DisplayStyle.None;
            });
            ab_mold.Init(ABMold.Hybrid);
            ab_mold.value = ABMold.Hybrid;

            root.Q<Button>("All").clicked += () =>
            {
                SelectList(true);
                RefreshAssetBundleList(folder_list, label_page);
            };
            root.Q<Button>("NotAll").clicked += () =>
            {
                SelectList(false);
                RefreshAssetBundleList(folder_list, label_page);
            };

            root.Q<Button>("BuildAndCopyDll").clicked += () =>
            {
                BuildAndCopyDll((BuildTarget)build_target.value);
            };
            root.Q<Button>("AutoBuildAllAssetBuildBundle").clicked += () =>
            {
                AutoBuildAssetBundle((BuildTarget)build_target.value);
            };
            root.Q<Button>("DeleteAllAssetBundle").clicked += () =>
            {
                DeleteAssetBundle((BuildTarget)build_target.value);
            };
            root.Q<Button>("RemoveAllAssetsBundleLabels").clicked += () =>
            {
                RemoveAllAssetBundleLabels();
            };
            root.Q<Button>("SetAllAssetBundleLabels").clicked += () =>
            {
                SetAssetBundleLabels();
            };
            root.Q<Button>("BuildAllAssetBuildBundle").clicked += () =>
            {
                BuildAllAssetBundles((BuildTarget)build_target.value);
            };
            root.Q<Button>("CreateMD5File").clicked += () =>
            {
                CreateMD5File((BuildTarget)build_target.value);
            };
        }
        private void RefreshAssetBundleList(ScrollView folder_list, Label label_page)
        {
            folder_list.Clear();
            for (int i = 0; i < m_DataList.Count; i++)
            {
                int index = i;
                Toggle toggle = new Toggle(m_DataList[index]);
                toggle.value = m_ExportList[index];
                toggle.RegisterCallback<ChangeEvent<bool>>((ent) =>
                {
                    m_ExportList[index] = ent.newValue;
                    label_page.text = $"打包 : {SelectCount()} / {m_ExportList.Count}";
                });
                folder_list.Add(toggle);
            }

            label_page.text = $"打包 : {SelectCount()} / {m_ExportList.Count}";
        }
        private void Init()
        {
            AppConfig = Resources.Load<AppConfig>(configPath);

            buildPath = "Assets/Resources/AssetsFolder";
            outputPath = Application.dataPath.Replace("Assets", "AssetBundle");
        }

        private void SetBuildPath(ABMold mold)
        {
            this.mold = mold;
            buildPath = mold == ABMold.Hybrid
                ? buildPath.Replace("AssetsFolder", "HybridFolder")
                : buildPath.Replace("HybridFolder", "AssetsFolder");
            GetFolders_Layer1();
        }

        private void SelectList(bool value)
        {
            for (int i = 0; i < m_ExportList.Count; i++)
                m_ExportList[i] = value;
        }

        private void SetResVersion(string ResVersion)
        {
            AppConfig.ResVersion = ResVersion;
            EditorUtility.SetDirty(AppConfig);
        }
        private string GetResVersion()
        {
            return AppConfig.ResVersion;
        }

        private void AutoBuildAssetBundle(BuildTarget buildTarget)
        {
            if (mold == ABMold.Hybrid)
            {
                BuildAndCopyDll(buildTarget);
            }
            RemoveAllAssetBundleLabels();
            SetAssetBundleLabels();
            DeleteAssetBundle(buildTarget);
            BuildAllAssetBundles(buildTarget);
            CreateMD5File(buildTarget);
            CopyABToBuildVersion(buildTarget, outputPath);
            if (AppConfig.LoadAssetsMold == LoadAssetsMold.Local)
            {
                CopyABToBuildVersion(buildTarget, $"{Application.streamingAssetsPath}/AssetBundle");
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private void CopyABToBuildVersion(BuildTarget buildTarget, string headPath)
        {
            string copyPath = GetCacheOutputPath(buildTarget);
            string targetPath = GetOutputPath(buildTarget, headPath);
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }
            string dir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            FileUtil.CopyFileOrDirectory(copyPath, targetPath);
            EditorUtility.RevealInFinder(targetPath);
        }

        private void BuildAndCopyDll(BuildTarget buildTarget)
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            PrebuildCommand.GenerateAll();
            CopyDLLToSourceData(buildTarget);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }


        private void CopyDLLToSourceData(BuildTarget target)
        {
            string targetDstDir = $"{Application.dataPath}/Resources/HybridFolder/App/Dll";
            CopyAOTAssembliesToSourceData(target, targetDstDir);
            CopyMyAssembliesToSourceData(target, targetDstDir);
        }
        private void CopyAOTAssembliesToSourceData(BuildTarget target, string targetDstDir)
        {
            string aotAssembliesSrcDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            Directory.CreateDirectory(targetDstDir);

            List<string> list = new List<string>();
            list.AddRange(Global.AOTMetaAssemblyNames);
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

        private void CopyMyAssembliesToSourceData(BuildTarget target, string targetDstDir)
        {
            string aotAssembliesSrcDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            Directory.CreateDirectory(targetDstDir);

            List<string> list = new List<string>();
            list.AddRange(Global.HotfixAssemblyNames);
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

        private void RemoveAllAssetBundleLabels()
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
        private void SetAssetBundleLabels()
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
        private void OnSceneFileSystemInfo(FileSystemInfo fileSystemInfo, string sceneName,
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
        private void SetLabels(FileInfo fileInfo, string sceneName, Dictionary<string, string> namePathDic)
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
        private string GetBundleName(FileInfo fileInfo, string sceneName)
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
        private void BuildAllAssetBundles(BuildTarget buildTarget)
        {
            string outPath = GetCacheOutputPath(buildTarget);
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
                    Debug.Log("AssetBundle 打包成功! " + outPath);
                }
            }
            else
            {
                ScriptableAssetBundleManifest = ScriptableBuildAssetBundles(outPath, false, true, buildTarget);
                if (ScriptableAssetBundleManifest)
                {
                    AssetDatabase.Refresh();
                    Debug.Log("AssetBundle 打包成功! " + outPath);
                }
            }
        }

        private CompatibilityAssetBundleManifest ScriptableBuildAssetBundles(string outputPath, bool forceRebuild,
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
        private void DeleteAssetBundle(BuildTarget buildTarget)
        {
            string outPath = GetCacheOutputPath(buildTarget);
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

        private CompatibilityAssetBundleManifest ScriptableAssetBundleManifest = null;
        private AssetBundleManifest AssetBundleManifest = null;
        //[MenuItem("AssetBundle/CreateMD5File(生成MD5文件)")]
        private void CreateMD5File(BuildTarget buildTarget)
        {
            string outPath = GetCacheOutputPath(buildTarget);
            string filePath = outPath + "/AssetBundleConfig.json";

            AssetBundleConfig config = new AssetBundleConfig();
            config.GameVersion = Application.version;
            config.ResVersion = AppConfig.ResVersion;
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
        private void ListFiles(FileSystemInfo fileSystemInfo, ref List<string> list)
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
        private void ListFiles(FileSystemInfo fileSystemInfo, ref List<FileInfo> list)
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
        private string GetFileMD5(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            // 引入命名空间   using System.Security.Cryptography;
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] bt = md5.ComputeHash(fs);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bt.Length; i++)
            {
                sb.Append(bt[i].ToString("x2"));
            }

            fs.Close();
            md5.Dispose();
            return sb.ToString();
        }

        #endregion

        private string GetCacheOutputPath(BuildTarget buildTarget)
        {
            // OutputPath > 项目目录/AssetBundle/{buildTarget}/{Application.version}/{AppConfig.ResVersion}/{mold}
            // OutputPath > 项目目录/AssetBundle/BuildABCache/{mold}/{buildTarget}
            return $"{Application.dataPath.Replace("Assets", "AssetBundle/BuildABCache")}/{mold}/{buildTarget}";
        }
        private string GetOutputPath(BuildTarget buildTarget, string headPath)
        {
            // OutputPath > 项目目录/AssetBundle/{buildTarget}/{Application.version}/{AppConfig.ResVersion}/{mold}
            // OutputPath > 项目目录/AssetBundle/BuildABCache/{mold}/{buildTarget}
            return $"{headPath}/{buildTarget}/{Application.version}/{AppConfig.ResVersion}/{mold}";
        }

        public List<bool> m_ExportList = new List<bool>();
        public List<string> m_DataList = new List<string>();
        public Dictionary<string, bool> m_SelectFolderDic = new Dictionary<string, bool>();

        public void Init_SelectFolderDic()
        {
            m_SelectFolderDic.Clear();
            for (int i = 0; i < m_ExportList.Count; i++)
            {
                if (m_ExportList[i] && !m_SelectFolderDic.ContainsKey(m_DataList[i]))
                    m_SelectFolderDic.Add(m_DataList[i], true);
            }
        }

        private void GetFolders_Layer1()
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

        private int SelectCount()
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