using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CopyFolderKeepAssetsUsingEditor
{
    [MenuItem("Assets/复制文件夹(复制依赖关系) %#D", false, 0)]
    static public void CopyFolderKeepAssetsUsing()
    {
        string oldFolderPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
        string[] s = oldFolderPath.Split('/');
        string folderName = s[s.Length - 1];
        if (folderName.Contains("."))
        {
            Debuger.LogError("该索引不是文件夹名字");
            return;
        }
        string copyedFolderPath = Path.GetFullPath(".") + Path.DirectorySeparatorChar + oldFolderPath;
        string tempFolderPath = Application.dataPath.Replace("Assets", "TempAssets") + SplitStr(oldFolderPath) + "_Copy";
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
            string directionPathTemp = Path.Combine(destDirectory, directionPath.Substring(sourceDirectory.Length + 1));// destDirectory + "\\" + directionPath.Substring(sourceDirectory.Length + 1);
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
            string filePathTemp = Path.Combine(destDirectory, filePath.Substring(sourceDirectory.Length + 1));// destDirectory + "\\" + filePath.Substring(sourceDirectory.Length + 1);
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
    private static readonly string[] kDefaultFileExtensions = {  
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
            EditorUtility.DisplayProgressBar("Scanning Assets folder", MakeRelativePath(_assetsPath, filePath), counter / (float)filesPaths.Count);
            string contents = string.Empty;
            try
            {
                contents = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Debuger.LogError(filePath);
                Debuger.LogError(e.ToString());
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
            EditorUtility.DisplayProgressBar("Regenerating GUIDs", MakeRelativePath(_assetsPath, filePath), counter / (float)guidsInFileMapKeysCount);
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
            FileManager.CreateFile(filePath, System.Text.Encoding.UTF8.GetBytes(contents));
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
