using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using App.Core.Tools;
using App.Editor.Helper;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using YooAsset.Editor;
using Debug = UnityEngine.Debug;

namespace App.Editor.Tools
{
    public class MenuToolsEditor : AssetModificationProcessor
    {
        private const int MENU_LEVEL = 1;
        
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
            head.AppendLine($" * author      : {EditorHelper.GetGitConfig("user.name")}");
            head.AppendLine($" * e-mail      : {EditorHelper.GetGitConfig("user.email")}");
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
        
        #region 拷贝模板脚本到Unity脚本模板路径 Win(Ctrl+Shift+E) Mac(Cmd+Shift+E)
        
        private static string[] watchers = new[]
        {
            "Assets/Settings/AssetBundleCollectorSetting.asset"
        };
        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                // 生成资源包枚举
                if (path == watchers[0])
                {
                    UpdateAssetPackage();
                }
            }
            return paths;
        }
        private static StringBuilder sb = new StringBuilder(1024);
        private static string target_path = $"{Application.dataPath}/App/Scripts/Runtime/Helper/Asset";

        [MenuItem("App/Editor/UpdateAssetPackage %#E", false, MENU_LEVEL)]
        private static void UpdateAssetPackage()
        {
            sb.Length = 0;
            var config = AssetDatabase.LoadAssetAtPath<AssetBundleCollectorSetting>(watchers[0]);
            sb.AppendLine("public enum AssetPackage");
            sb.AppendLine("{");
            foreach (var package in config.Packages)
            {
                sb.AppendLine($"    {package.PackageName},");
            }
            sb.AppendLine("}");
            File.WriteAllText($"{target_path}/AssetPackage.cs", sb.ToString());
            AssetDatabase.Refresh();
        }

        #endregion

        #region 拷贝模板脚本到Unity脚本模板路径 Win(Ctrl+Shift+T) Mac(Cmd+Shift+T)

        [MenuItem("App/Editor/CopyTemplateScripts %#T", false, MENU_LEVEL)]
        public static void CopyTemplateScripts()
        {
            var template_script_path = $"{Application.dataPath}/{EditorHelper.BaseEditorPath()}/Tools/ScriptTemplates";
            var base_path = "";
#if UNITY_EDITOR_WIN
            base_path = $"{AppDomain.CurrentDomain.BaseDirectory}/Data";
#elif UNITY_EDITOR_OSX
            base_path = $"{AppDomain.CurrentDomain.BaseDirectory}/Unity.app/Contents";
#endif
            if (base_path != string.Empty)
            {
                var install_path = $"{base_path}/Resources/ScriptTemplates";
                EditorHelper.Copy(template_script_path, install_path, "txt");
            }
            var result = EditorUtility.DisplayDialog("重要提示", "脚本模版拷贝成功，重启Unity Editor生效", "重启", "取消");
            if (result)
            {
                EditorApplication.OpenProject($"{Application.dataPath.Replace("Assets", string.Empty)}");
            }
        }

        #endregion

        #region Protobuf2CS Win(Ctrl+Shift+Y) Mac(Cmd+Shift+Y)

        
        [MenuItem("App/Editor/Protobuf2CS %#Y", false, MENU_LEVEL)]
        public static void Protobuf2CS()
        {
            var cdPath = Application.dataPath.Replace("Assets", "Data/protobuf");
#if UNITY_EDITOR_WIN
            var file = $"cmd.exe";
            var arguments = $"/C cd /D {cdPath} && proto2csharp.bat"; //非得cd到某目录,再调用下bat文件,不能直接使用全目录
#endif
#if UNITY_EDITOR_OSX
            var file = $"/bin/sh";
            var arguments = $"{cdPath}/proto2csharp.sh";
#endif
            var startInfo = new ProcessStartInfo
            {
                FileName = file,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            var process = new Process { StartInfo = startInfo };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Debug.Log(output);
            AssetDatabase.Refresh();
        }

        #endregion
        
        #region 更新资源路径配置文件（自动/手动）Win(Ctrl+Shift+R) Mac(Cmd+Shift+R)
        
        [DidReloadScripts]
        [MenuItem("App/Editor/UpdateAssetPath %#R", false, MENU_LEVEL)]
        public static void UpdateAssetPath()
        {
            sb.Length = 0;
            var files = new List<FileInfo>();
            sb.AppendLine("public class AssetPath");
            sb.AppendLine("{");
            
            sb.AppendLine("    public const string Global = \"Global\";");
            GetConst("Builtin", ref sb);
            GetConst("Hotfix", ref sb);
            
            sb.AppendLine("}");
            File.WriteAllText($"{target_path}/AssetPath.cs", sb.ToString());
            AssetDatabase.Refresh();
        }

        private static void GetConst(string folder, ref StringBuilder stringBuilder)
        {
            var files = GetFiles(folder);
            foreach (var file in files)
            {
                var key = file.Name.Split('.')[0];
                var value = file.FullName.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                stringBuilder.AppendLine($"    public const string {key} = \"{value}\";");
            }
        }
        
        private static readonly string[] IgnoreExtensions = { ".dll", ".shader", ".shadergraph", ".shadervariants", ".meta", ".meta", ".DS_Store", ".bytes" };
        private static List<FileInfo> GetFiles(string folder)
        {
            var path = $"{Application.dataPath}/Bundles/{folder}";
            var fileInfos = new List<FileInfo>();
            if (Directory.Exists(path))
            {
                var direction = new DirectoryInfo(path);
                var files = direction.GetFiles("*", SearchOption.AllDirectories);
                foreach (var t in files)
                {
                    if(IgnoreExtensions.Contains(t.Extension)) continue;
                    fileInfos.Add(t);
                }
            }

            return fileInfos;
        }
        
        #endregion
        
        #region 打开默认路径
        
        [MenuItem("Assets/Open Folder/DataPath", false, 0)]
        public static void OpenDataFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.dataPath}");
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", $"-R {Application.dataPath}");
#endif
        }

        [MenuItem("Assets/Open Folder/PersistentDataPath", false, 0)]
        public static void OpenPersistentDataFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.persistentDataPath}");
#elif UNITY_EDITOR_OSX
            var url = new System.Uri(Application.persistentDataPath).AbsoluteUri;
            UnityEngine.Application.OpenURL(url);
#endif
        }

        [MenuItem("Assets/Open Folder/StreamingAssetsPath", false, 0)]
        public static void OpenStreamingAssetsFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.streamingAssetsPath}");
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", $"-R {Application.streamingAssetsPath}");
#endif
        }

        [MenuItem("Assets/Open Folder/TemporaryCachePath", false, 0)]
        public static void OpenTemporaryCacheFolder()
        {
#if UNITY_EDITOR_WIN
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.temporaryCachePath}");
#elif UNITY_EDITOR_OSX
            System.Diagnostics.Process.Start("open", $"-R {Application.temporaryCachePath}");
#endif
        }
        
        #endregion

        #region 复制文件依赖关系 Win(Ctrl+Shift+D) Mac(Cmd+Shift+D)

        [MenuItem("Assets/复制文件夹(复制依赖关系) %#D", false, 0)]
        public static void CopyFolderKeepAssetsUsing()
        {
            var oldFolderPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            var s = oldFolderPath.Split('/');
            var folderName = s[^1];
            if (folderName.Contains("."))
            {
                Debug.LogError("该索引不是文件夹名字");
                return;
            }

            var copyedFolderPath = Path.GetFullPath(".") + Path.DirectorySeparatorChar + oldFolderPath;
            var tempFolderPath = Application.dataPath.Replace("Assets", "TempAssets") + SplitStr(oldFolderPath) +
                                 "_Copy";
            var newFoldrPath = tempFolderPath.Replace("TempAssets", "Assets");

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
            var str = "";
            for (var i = 0; i < path.Split('/').Length; i++)
            {
                if (i != 0)
                {
                    var _str = "/" + path.Split('/')[i];
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

        private static void RegenerateGuids(string _assetsPath, string[] regeneratedExtensions = null)
        {
            regeneratedExtensions ??= kDefaultFileExtensions;

            // Get list of working files  
            var filesPaths = new List<string>();
            foreach (var extension in regeneratedExtensions)
            {
                filesPaths.AddRange(
                    Directory.GetFiles(_assetsPath, extension, SearchOption.AllDirectories)
                );
            }

            // Create dictionary to hold old-to-new GUID map  
            var guidOldToNewMap = new Dictionary<string, string>();
            var guidsInFileMap = new Dictionary<string, List<string>>();

            // We must only replace GUIDs for Resources present in Assets.   
            // Otherwise built-in resources (shader, meshes etc) get overwritten.  
            var ownGuids = new HashSet<string>();

            // Traverse all files, remember which GUIDs are in which files and generate new GUIDs  
            var counter = 0;
            foreach (var filePath in filesPaths)
            {
                EditorUtility.DisplayProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath),
                    counter / (float)filesPaths.Count);
                string contents;
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

                var guids = GetGuids(contents);
                var isFirstGuid = true;
                foreach (var oldGuid in guids)
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
                        var newGuid = Guid.NewGuid().ToString("N");
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
            var guidsInFileMapKeysCount = guidsInFileMap.Keys.Count;
            foreach (var filePath in guidsInFileMap.Keys)
            {
                EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath),
                    counter / (float)guidsInFileMapKeysCount);
                counter++;

                var contents = File.ReadAllText(filePath);
                foreach (var oldGuid in guidsInFileMap[filePath])
                {
                    if (!ownGuids.Contains(oldGuid))
                        continue;

                    var newGuid = guidOldToNewMap[oldGuid];
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
            var textLength = text.Length;
            var guidStartLength = guidStart.Length;
            var guids = new List<string>();

            var index = 0;
            while (index + guidStartLength + guidLength < textLength)
            {
                index = text.IndexOf(guidStart, index, StringComparison.Ordinal);
                if (index == -1)
                    break;

                index += guidStartLength;
                var guid = text.Substring(index, guidLength);
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
            foreach (var c in text)
            {
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
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }

        #endregion

        #endregion
    }
}