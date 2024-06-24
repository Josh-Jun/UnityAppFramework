using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AppFrame.Config;
using AppFrame.Tools;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AppFrame.Editor
{
    public class MenuToolsEditor : UnityEditor.AssetModificationProcessor
    {
        #region 脚本模板导入修改命名空间

        private static string[] temps = { "Logic", "View" };
        private static StringBuilder head = new StringBuilder(1024);
        /// <summary>  
        /// 此函数在asset被创建完，文件已经生成到磁盘上，但是没有生成.meta文件和import之前被调用  
        /// </summary>  
        /// <param name="newFileMeta">newfilemeta 是由创建文件的path加上.meta组成的</param>  
        public static void OnWillCreateAsset(string newFileMeta)
        {
            head.Length = 0;
            head.AppendLine("/* *");
            head.AppendLine(" * ===============================================");
            head.AppendLine($" * author      : {EditorTool.GetGitConfig("user.name")}");
            head.AppendLine($" * e-mail      : {EditorTool.GetGitConfig("user.email")}");
            head.AppendLine($" * create time : {DateTime.Now.Year}年{DateTime.Now.Month}月{DateTime.Now.Day} {DateTime.Now.Hour}:{DateTime.Now.Minute}");
            head.AppendLine(" * function    : ");
            head.AppendLine(" * ===============================================");
            head.AppendLine(" * */");
            
            var newFilePath = newFileMeta.Replace(".meta", "");
            if (Path.GetExtension(newFilePath) == ".txt" || Path.GetExtension(newFilePath) == ".cs")
            {
                var realPath = Application.dataPath.Replace("Assets", "") + newFilePath;
                if (File.Exists(realPath))
                {
                    var scriptContent = File.ReadAllText(realPath);
                    // 这里实现自定义的一些规则
                    // scriptContent = scriptContent.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(newFilePath));
                    var namespaces = Path.GetFileNameWithoutExtension(newFilePath);
                    for (int i = 0; i < temps.Length; i++)
                    {
                        namespaces = namespaces.Replace(temps[i], "");
                    }

                    scriptContent = scriptContent.Replace("#NAMESPACE#", namespaces);
                    scriptContent = scriptContent.Insert(0, head.ToString());

                    File.WriteAllText(realPath, scriptContent);
                }
            }
        }

        #endregion

        #region 拷贝模板脚本到Unity脚本模板路径

        [MenuItem("Tools/Editor/CopyTemplateScripts", false, 0)]
        public static void CopyTemplateScripts()
        {
            var template_script_path = $"{Application.dataPath}/AppFrame/Editor/Tools/ScriptTemplates";
            var base_path = string.Empty;
#if UNITY_EDITOR_WIN
            base_path = $"{AppDomain.CurrentDomain.BaseDirectory}/Data";
#elif UNITY_EDITOR_OSX
            base_path = $"{AppDomain.CurrentDomain.BaseDirectory}/Unity.app/Contents";
#endif
            if (base_path != string.Empty)
            {
                var install_path = $"{base_path}/Resources/ScriptTemplates";
                EditorTool.Copy(template_script_path, install_path, "txt");
            }
            var result = EditorUtility.DisplayDialog("重要提示", "脚本模版拷贝成功，重启Unity Editor生效", "重启", "取消");
            if (result)
            {
                EditorApplication.OpenProject($"{Application.dataPath.Replace("Assets", string.Empty)}");
            }
        }

        #endregion
        
        #region 更新资源路径配置文件（自动/手动）
        
        [DidReloadScripts]
        [MenuItem("Tools/Editor/UpdateAssetPathConfig", false, 0)]
        public static void UpdateAssetPathConfig()
        {
            var config = Resources.Load<AssetPathConfig>("AssetsFolder/Global/AssetConfig/AssetPathConfig");
            config.AssetPath.Clear();
            var folder = "AssetsFolder";
            List<FileInfo> files = GetFiles(folder);
            for (int i = 0; i < files.Count; i++)
            {
                var key = files[i].Name.Split('.')[0];
                
                var value = 
                    files[i].FullName.Substring(files[i].FullName.IndexOf(folder)).Replace('\\', '/').Split('.')[0].Replace($"{folder}/", "");
                if (!value.Contains("/")) continue;

                AssetPath ap = new AssetPath
                {
                    name = key,
                    path = value,
                };
                config.AssetPath.Add(ap);
            }
            EditorUtility.SetDirty(config);
            AssetDatabase.Refresh();
        }
        
        private static string[] IncludeMarks = { "Views", "Configs", "Scenes", "Prefabs" };
        private static List<FileInfo> GetFiles(string folder)
        {
            var path = $"{Application.dataPath}/Resources/{folder}";
            List<FileInfo> fileInfos = new List<FileInfo>();
            if (Directory.Exists(path))
            {
                DirectoryInfo direction = new DirectoryInfo(path);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].FullName.Contains("Scene") && !files[i].Name.EndsWith(".unity")) continue; //剔除场景以外的文件
                    if (files[i].Name.EndsWith(".meta")) continue; //剔除.meta文件
                    if (files[i].FullName.Contains("Shader")) continue; //剔除Shader
                    if (files[i].FullName.Contains("Dll")) continue; //剔除Dll
                    if (files[i].FullName.Contains("Test")) continue; //剔除Test
                    var isContinue = true;
                    for (int j = 0; j < IncludeMarks.Length; j++)
                    {
                        if (files[i].FullName.Contains(IncludeMarks[j]))
                        {
                            isContinue = false;
                            break;
                        }
                    }
                    if (isContinue) continue; //
                    fileInfos.Add(files[i]);
                }
            }

            return fileInfos;
        }
        
        #endregion
        
        #region 打开默认路径
        
        [MenuItem("Tools/OpenFolder/DataPath", false, 0)]
        public static void OpenDataFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.dataPath}");
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", $"-R {Application.dataPath}");
#endif
        }

        [MenuItem("Tools/OpenFolder/PersistentDataPath", false, 0)]
        public static void OpenPersistentDataFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.persistentDataPath}");
#elif UNITY_EDITOR_OSX
            var url = new System.Uri(Application.persistentDataPath).AbsoluteUri;
            UnityEngine.Application.OpenURL(url);
#endif
        }

        [MenuItem("Tools/OpenFolder/StreamingAssetsPath", false, 0)]
        public static void OpenStreamingAssetsFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.streamingAssetsPath}");
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", $"-R {Application.streamingAssetsPath}");
#endif
        }

        [MenuItem("Tools/OpenFolder/TemporaryCachePath", false, 0)]
        public static void OpenTemporaryCacheFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.temporaryCachePath}");
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", $"-R {Application.temporaryCachePath}");
#endif
        }
        
        #endregion

        #region 复制文件依赖关系

        [MenuItem("Assets/复制文件夹(复制依赖关系) %#D", false, 0)]
        public static void CopyFolderKeepAssetsUsing()
        {
            string oldFolderPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            string[] s = oldFolderPath.Split('/');
            string folderName = s[s.Length - 1];
            if (folderName.Contains("."))
            {
                Debug.LogError("该索引不是文件夹名字");
                return;
            }

            string copyedFolderPath = Path.GetFullPath(".") + Path.DirectorySeparatorChar + oldFolderPath;
            string tempFolderPath = Application.dataPath.Replace("Assets", "TempAssets") + SplitStr(oldFolderPath) +
                                    "_Copy";
            string newFoldrPath = tempFolderPath.Replace("TempAssets", "Assets");

            CopyDirectory(copyedFolderPath, tempFolderPath);
            //重新生成guids
            RegenerateGuids(copyedFolderPath);
            CopyDirectory(copyedFolderPath, newFoldrPath);
            AssetDatabase.DeleteAsset(oldFolderPath);
            CopyDirectory(tempFolderPath, copyedFolderPath);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private static string SplitStr(string path)
        {
            string str = "";
            for (int i = 0; i < path.Split('/').Length; i++)
            {
                if (i != 0)
                {
                    string _str = "/" + path.Split('/')[i];
                    str += _str;
                }
            }

            return str;
        }

        #region Copy

        public static void CopyDirectory(string sourceDirectory, string destDirectory)
        {
            //判断源目录和目标目录是否存在，如果不存在，则创建一个目录
            if (!Directory.Exists(sourceDirectory))
            {
                Directory.CreateDirectory(sourceDirectory);
            }

            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            //拷贝文件
            CopyFile(sourceDirectory, destDirectory);
            //拷贝子目录       
            //获取所有子目录名称
            string[] directionName = Directory.GetDirectories(sourceDirectory);
            foreach (string directionPath in directionName)
            {
                //根据每个子目录名称生成对应的目标子目录名称
                string directionPathTemp =
                    Path.Combine(destDirectory,
                        directionPath.Substring(sourceDirectory.Length +
                                                1)); // destDirectory + "\\" + directionPath.Substring(sourceDirectory.Length + 1);
                //递归下去
                CopyDirectory(directionPath, directionPathTemp);
            }
        }

        public static void CopyFile(string sourceDirectory, string destDirectory)
        {
            //获取所有文件名称
            string[] fileName = Directory.GetFiles(sourceDirectory);
            foreach (string filePath in fileName)
            {
                //根据每个文件名称生成对应的目标文件名称
                string filePathTemp =
                    Path.Combine(destDirectory,
                        filePath.Substring(sourceDirectory.Length +
                                           1)); // destDirectory + "\\" + filePath.Substring(sourceDirectory.Length + 1);
                //若不存在，直接复制文件；若存在，覆盖复制
                if (File.Exists(filePathTemp))
                {
                    File.Copy(filePath, filePathTemp, true);
                }
                else
                {
                    File.Copy(filePath, filePathTemp);
                }
            }
        }

        #endregion

        #region GUID

        private static readonly string[] kDefaultFileExtensions =
        {
            // "*.meta",  
            // "*.mat",  
            // "*.anim",  
            // "*.prefab",  
            // "*.unity",  
            // "*.asset"  
            "*.*"
        };

        static public void RegenerateGuids(string _assetsPath, string[] regeneratedExtensions = null)
        {
            if (regeneratedExtensions == null)
            {
                regeneratedExtensions = kDefaultFileExtensions;
            }

            // Get list of working files  
            List<string> filesPaths = new List<string>();
            foreach (string extension in regeneratedExtensions)
            {
                filesPaths.AddRange(
                    Directory.GetFiles(_assetsPath, extension, SearchOption.AllDirectories)
                );
            }

            // Create dictionary to hold old-to-new GUID map  
            Dictionary<string, string> guidOldToNewMap = new Dictionary<string, string>();
            Dictionary<string, List<string>> guidsInFileMap = new Dictionary<string, List<string>>();

            // We must only replace GUIDs for Resources present in Assets.   
            // Otherwise built-in resources (shader, meshes etc) get overwritten.  
            HashSet<string> ownGuids = new HashSet<string>();

            // Traverse all files, remember which GUIDs are in which files and generate new GUIDs  
            int counter = 0;
            foreach (string filePath in filesPaths)
            {
                EditorUtility.DisplayProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath),
                    counter / (float)filesPaths.Count);
                string contents = string.Empty;
                try
                {
                    contents = File.ReadAllText(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError(filePath);
                    Debug.LogError(e.ToString());
                    counter++;
                    continue;
                }

                IEnumerable<string> guids = GetGuids(contents);
                bool isFirstGuid = true;
                foreach (string oldGuid in guids)
                {
                    // First GUID in .meta file is always the GUID of the asset itself  
                    if (isFirstGuid && Path.GetExtension(filePath) == ".meta")
                    {
                        ownGuids.Add(oldGuid);
                        isFirstGuid = false;
                    }

                    // Generate and save new GUID if we haven't added it before  
                    if (!guidOldToNewMap.ContainsKey(oldGuid))
                    {
                        string newGuid = Guid.NewGuid().ToString("N");
                        guidOldToNewMap.Add(oldGuid, newGuid);
                    }

                    if (!guidsInFileMap.ContainsKey(filePath))
                        guidsInFileMap[filePath] = new List<string>();

                    if (!guidsInFileMap[filePath].Contains(oldGuid))
                    {
                        guidsInFileMap[filePath].Add(oldGuid);
                    }
                }

                counter++;
            }

            // Traverse the files again and replace the old GUIDs  
            counter = -1;
            int guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;
            foreach (string filePath in guidsInFileMap.Keys)
            {
                EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath),
                    counter / (float)guidsInFileMapKeysCount);
                counter++;

                string contents = File.ReadAllText(filePath);
                foreach (string oldGuid in guidsInFileMap[filePath])
                {
                    if (!ownGuids.Contains(oldGuid))
                        continue;

                    string newGuid = guidOldToNewMap[oldGuid];
                    if (string.IsNullOrEmpty(newGuid))
                        throw new NullReferenceException("newGuid == null");

                    contents = contents.Replace("guid: " + oldGuid, "guid: " + newGuid);
                }

                //File.WriteAllText(filePath, contents);
                FileTools.CreateFile(filePath, System.Text.Encoding.UTF8.GetBytes(contents));
            }

            EditorUtility.ClearProgressBar();
        }

        private static IEnumerable<string> GetGuids(string text)
        {
            const string guidStart = "guid: ";
            const int guidLength = 32;
            int textLength = text.Length;
            int guidStartLength = guidStart.Length;
            List<string> guids = new List<string>();

            int index = 0;
            while (index + guidStartLength + guidLength < textLength)
            {
                index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
                if (index == -1)
                    break;

                index += guidStartLength;
                string guid = text.Substring(index, guidLength);
                index += guidLength;

                if (IsGuid(guid))
                {
                    guids.Add(guid);
                }
            }

            return guids;
        }

        private static bool IsGuid(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (
                    !((c >= '0' && c <= '9') ||
                      (c >= 'a' && c <= 'z'))
                )
                    return false;
            }

            return true;
        }

        private static string MakeRelativePath(string fromPath, string toPath)
        {
            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }

        #endregion

        #endregion
    }
}