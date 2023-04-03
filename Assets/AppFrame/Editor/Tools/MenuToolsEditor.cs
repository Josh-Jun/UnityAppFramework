using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AppFrame.Tools;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AppFrame.Editor
{
    public class MenuToolsEditor : UnityEditor.AssetModificationProcessor
    {
        #region 脚本模板导入修改命名空间

        private static string[] temps = { "Logic", "View" };

        /// <summary>  
        /// 此函数在asset被创建完，文件已经生成到磁盘上，但是没有生成.meta文件和import之前被调用  
        /// </summary>  
        /// <param name="newFileMeta">newfilemeta 是由创建文件的path加上.meta组成的</param>  
        public static void OnWillCreateAsset(string newFileMeta)
        {
            var newFilePath = newFileMeta.Replace(".meta", "");
            if (Path.GetExtension(newFilePath) == ".txt" || Path.GetExtension(newFilePath) == ".cs")
            {
                var realPath = Application.dataPath.Replace("Assets", "") + newFilePath;
                var scriptContent = File.ReadAllText(realPath);
                // 这里实现自定义的一些规则
                // scriptContent = scriptContent.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(newFilePath));
                var namespaces = Path.GetFileNameWithoutExtension(newFilePath);
                for (int i = 0; i < temps.Length; i++)
                {
                    namespaces = namespaces.Replace(temps[i], "");
                }

                scriptContent = scriptContent.Replace("#NAMESPACE#", namespaces);

                File.WriteAllText(realPath, scriptContent);
            }
        }

        #endregion

        #region Debug封装重定位

        private static bool m_hasForceMono = false;
        private static string LogCS = "Log.cs";
        // 处理asset打开的callback函数
        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        static bool OnOpenAsset(int instance, int line)
        {
            if (m_hasForceMono) return false;

            // 自定义函数，用来获取log中的stacktrace，定义在后面。
            string stack_trace = GetStackTrace();
            // 通过stacktrace来定位是否是我们自定义的log，我的log中有特殊文字[SDebug]，很好识别
            if (!string.IsNullOrEmpty(stack_trace))
            {
                // 正则匹配at xxx，在第几行
                Match matches = Regex.Match(stack_trace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
                string pathline = "";
                while (matches.Success)
                {
                    pathline = matches.Groups[1].Value;

                    // 找到不是我们自定义log文件的那行，重新整理文件路径，手动打开
                    if (!pathline.Contains("Log.cs") && !string.IsNullOrEmpty(pathline))
                    {
                        int split_index = pathline.LastIndexOf(":");
                        string path = pathline.Substring(0, split_index);
                        line = Convert.ToInt32(pathline.Substring(split_index + 1));
                        m_hasForceMono = true;
                        //方式一
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), line);
                        m_hasForceMono = false;
                        //方式二
                        //string fullpath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                        // fullpath = fullpath + path;
                        //  UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullpath.Replace('/', '\\'), line);
                        return true;
                    }

                    matches = matches.NextMatch();
                }

                return true;
            }

            return false;
        }


        static string GetStackTrace()
        {
            // 找到类UnityEditor.ConsoleWindow
            var type_console_window = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            // 找到UnityEditor.ConsoleWindow中的成员ms_ConsoleWindow
            var filedInfo =
                type_console_window.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            // 获取ms_ConsoleWindow的值
            var ConsoleWindowInstance = filedInfo.GetValue(null);
            if (ConsoleWindowInstance != null)
            {
                if ((object)EditorWindow.focusedWindow == ConsoleWindowInstance)
                {
                    // 找到类UnityEditor.ConsoleWindow中的成员m_ActiveText
                    filedInfo = type_console_window.GetField("m_ActiveText",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    string activeText = filedInfo.GetValue(ConsoleWindowInstance).ToString();
                    return activeText;
                }
            }

            return null;
        }

        #endregion
        
        [MenuItem("Tools/OpenFolder/DataPath", false, 0)]
        public static void OpenDataFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.dataPath}");
        }

        [MenuItem("Tools/OpenFolder/PersistentDataPath", false, 0)]
        public static void OpenPersistentDataFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.persistentDataPath}");
        }

        [MenuItem("Tools/OpenFolder/StreamingAssetsPath", false, 0)]
        public static void OpenStreamingAssetsFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.streamingAssetsPath}");
        }

        [MenuItem("Tools/OpenFolder/TemporaryCachePath", false, 0)]
        public static void OpenTemporaryCacheFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", $"file://{Application.temporaryCachePath}");
        }

        [MenuItem("Tools/CreateAssetsPath", false, 3)]
        public static void CreateAssetsPath()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("namespace AppFrame.Data");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("    public class AssetsPathConfig");
            stringBuilder.AppendLine("    {");
            stringBuilder.Append(GetAllPath("HybridFolder"));
            stringBuilder.Append(GetAllPath("AssetsFolder"));
            stringBuilder.AppendLine("    }");
            stringBuilder.Append("}");

            string output = string.Format("{0}/AppFrame/Runtime/Frame/Tools/AssetsPathConfig.cs", Application.dataPath);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(stringBuilder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();

            AssetDatabase.Refresh();
        }

        private static string GetAllPath(string folder)
        {
            StringBuilder sb = new StringBuilder();
            List<FileInfo> files = GetFiles(folder);

            for (int i = 0; i < files.Count; i++)
            {
                var name = files[i].Name.Split('.')[0];
                if (sb.ToString().Contains(name)) continue;
                var value =
                    files[i].FullName.Substring(files[i].FullName.IndexOf(folder)).Replace('\\', '/').Split('.')[0]
                        .Replace($"{folder}/", "");
                if (files[i].FullName.Contains("Pico") || files[i].FullName.Contains("Mobile"))
                {
                    value = value.Replace("Mobile", "{0}").Replace("Pico", "{0}");
                }

                if (!value.Contains("/")) continue;
                sb.AppendLine($"        public const string {name} = \"{value}\";");
            }

            return sb.ToString();
        }

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
                    if (files[i].Name.EndsWith(".meta")) continue; //剔除.meta文件
                    if (files[i].Name.Contains("lua")) continue; //剔除lua文件
                    if (files[i].FullName.Contains("Scenes") && !files[i].Name.EndsWith(".unity")) continue; //剔除场景以外的文件
                    fileInfos.Add(files[i]);
                    Debug.Log(files[i].FullName);
                }
            }

            return fileInfos;
        }

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
    }
}