using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
/// <summary>
/// AssetBundle编辑
/// </summary>
public class AssetBundleWindowEditor : EditorWindow
{
    private static GUIStyle titleStyle;

    private BuildTarget buildTarget = BuildTarget.StandaloneWindows;
    private string outputPath = "Assets/StreamingAssets";
    private string buildPath = "Assets/AssetsFolder";
    private Dictionary<string, Dictionary<string, string>> sceneDic = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, string> desDic = new Dictionary<string, string>();

    private string version = "1";

    [MenuItem("Tools/Build AssetBundle")]
    public static void OpenWindow()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        AssetBundleWindowEditor window = GetWindow<AssetBundleWindowEditor>("BuildAssetBundleWindow");
        window.Show();
    }
    private void OnGUI()
    {
        EditorGUILayout.Space();

        GUILayout.Label(new GUIContent("Build AssetBundle"), titleStyle);

        // AssetBundle Labels.
        EditorGUILayout.Space();
        if (GUILayout.Button("Set AssetBundle Labels(自动做标记)"))
        {
            EditorApplication.delayCall += SetAssetBundleLabels;
        }
        EditorGUILayout.Space();
        GUILayout.BeginVertical();

        // build target.
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);
        EditorGUILayout.Space();

        // build path.
        buildPath = EditorGUILayout.TextField("Build Path", buildPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", buildPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                buildPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        // output path.
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", outputPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                outputPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        //版本号
        EditorGUILayout.Space();
        version = EditorGUILayout.TextField("Version ID", version);

        //更新描述
        if (desDic.Count > 0)
        {
            EditorGUILayout.Space();
            for (int i = 0; i < desDic.Keys.Count; i++)
            {
                desDic[desDic.Keys.ToArray()[i]] = EditorGUILayout.TextField(desDic.Keys.ToArray()[i] + " Update Des", desDic[desDic.Keys.ToArray()[i]]);
            }
        }

        // build.
        EditorGUILayout.Space();
        if (GUILayout.Button("BuildAllAssetBundle(一键打包)"))
        {
            EditorApplication.delayCall += BuildAllAssetBundles;
        }

        // MD5 file.
        EditorGUILayout.Space();
        if (GUILayout.Button("CreateMD5File(生成MD5文件)"))
        {
            EditorApplication.delayCall += CreateFile;
        }

        // delete.
        EditorGUILayout.Space();
        if (GUILayout.Button("DeleteAllAssetBundle(一键删除)"))
        {
            EditorApplication.delayCall += DeleteAssetBundle;
        }
        GUILayout.EndVertical();
    }

    #region 自动做标记
    //[MenuItem("AssetBundle/Set AssetBundle Labels(自动做标记)")]
    void SetAssetBundleLabels()
    {
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

                OnSceneFileSystemInfo(sceneDirectoryInfo, sceneName, namePathDic);

                //OnWriteConfig(sceneName, namePathDic);

                if (!sceneDic.ContainsKey(tempDirectoryInfo.Name))
                    sceneDic.Add(tempDirectoryInfo.Name, namePathDic);
                if (!desDic.ContainsKey(tempDirectoryInfo.Name))
                    desDic.Add(tempDirectoryInfo.Name, "");
            }
        }
        Debug.Log("AssetBundle Labels设置成功!");
    }

    /// <summary>
    /// 3. 遍历场景文件夹里的所有文件系统
    /// </summary>
    /// <param name="fileSystemInfo">文件</param>
    /// <param name="sceneName">场景名字</param>
    private void OnSceneFileSystemInfo(FileSystemInfo fileSystemInfo, string sceneName, Dictionary<string, string> namePathDic)
    {
        if (!fileSystemInfo.Exists)
            Debug.LogError(fileSystemInfo.FullName + "不存在！");

        DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
        // 获取所有文件系统[包括文件夹和文件]
        FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
        foreach (FileSystemInfo tempfileSystemInfo in fileSystemInfos)
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
        //Debug.Log(assetPath); // Assets\AssetsFolder\Scene1\Character\NB\Player4.prefab

        // 6.用 AssetImporter 类 ， 修改名称和后缀 ；
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath); // GetAtPath方法是获取Assets文件和之后的目录
        assetImporter.assetBundleName = bundleName.ToLower();
        if (fileInfo.Extension == ".unity")
            assetImporter.assetBundleVariant = "scene";
        else
            assetImporter.assetBundleVariant = "ab";

        // 添加到字典里 
        string folderName = "";
        if (bundleName.Contains("/"))
            folderName = bundleName.Split('/')[1]; // key
        else
            folderName = bundleName;
        string bundlePath = assetImporter.assetBundleName + "." + assetImporter.assetBundleVariant; // value
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
            return sceneName + "/" + tempPath;
        }
        else
        {
            // 如果是场景
            // Scene1.unity
            return sceneName;
        }
    }

    /// <summary>
    /// 7. 保存对应的文件夹名和具体路径 ；
    /// </summary>
    private void OnWriteConfig(string sceneName, Dictionary<string, string> namePathDic)
    {
        if (FileManager.FolderExist(Application.dataPath.Replace("Assets", "") + outputPath + "/" + PlatformManager.Instance.Name()))
        {
            FileManager.CreateFolder(Application.dataPath.Replace("Assets", "") + outputPath + "/" + PlatformManager.Instance.Name());
        }
        string path = Application.dataPath.Replace("Assets", "") + outputPath + "/" + PlatformManager.Instance.Name() + "/" + sceneName + "Record.txt";
        // Debug.Log(path); // D:/Unity_forWork/Unity_Project/AssetBundle02_Senior/Assets/AssetBundles/Scene3Record.config
        if (!File.Exists(path))
        {
            File.Create(path);
        }
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
        {
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(namePathDic.Count);

                foreach (var item in namePathDic)
                {
                    sw.WriteLine(item.Key + "--" + item.Value);
                }
            }
        }
    }
    #endregion

    #region 打包
    //[MenuItem("AssetBundle/BuildAllAssetBundle(一键打包)")]
    void BuildAllAssetBundles()
    {
        string outPath = Application.dataPath.Replace("Assets", "") + outputPath + "/" + PlatformManager.Instance.Name();
        if (!FileManager.FolderExist(outPath))
        {
            FileManager.CreateFolder(outPath);
        }
        BuildPipeline.BuildAssetBundles(outPath, BuildAssetBundleOptions.None, buildTarget);

        AssetDatabase.Refresh();
        Debug.Log("AssetBundle 打包成功!");
    }
    #endregion

    #region 删除
    //[MenuItem("AssetBundle/DeleteAllAssetBundle(一键删除)")]
    private void DeleteAssetBundle()
    {
        string outPath = Application.dataPath.Replace("Assets", "") + outputPath + "/" + PlatformManager.Instance.Name();
        if (!FileManager.FolderExist(outPath))
        {
            return;
        }
        Directory.Delete(outPath, true); // 删除 AssetBundle文件夹 , true 强势删除 ;
        File.Delete(outPath + ".meta");  // unity 自带的 .meta 文件也删掉[不删会报警告] ;
        AssetDatabase.Refresh();
    }
    #endregion

    #region 生成MD5文件
    //[MenuItem("AssetBundle/CreateMD5File(生成MD5文件)")]
    private void CreateFile()
    {
        // outPath = E:/Shuai/AssetBundle/Assets/StreamingAssets/Windows
        string outPath = Application.dataPath.Replace("Assets", "") + outputPath + "/" + PlatformManager.Instance.Name();
        string filePath = outPath + "/AssetBundleConfig.xml";
        if (File.Exists(filePath))
            File.Delete(filePath);

        XmlDocument xmlDocument = new XmlDocument();
        XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDocument.AppendChild(xmlDeclaration);
        var root = xmlDocument.CreateElement("AssetBundleConfig");
        root.SetAttribute("GameVersion", Application.version);
        foreach (var scene in sceneDic)
        {
            var xmlScene = xmlDocument.CreateElement("Scenes");
            xmlScene.SetAttribute("SceneName", scene.Key);
            xmlScene.SetAttribute("Version", version);
            xmlScene.SetAttribute("Des", desDic[scene.Key]);
            //root.AppendChild(xmlScene);
            foreach (var folder in scene.Value)
            {
                FileInfo file = new FileInfo(outPath + "/" + folder.Value);
                var xmlFolder = xmlDocument.CreateElement("Folders");
                xmlFolder.SetAttribute("FolderName", folder.Key);
                xmlFolder.SetAttribute("BundleName", folder.Value);
                xmlFolder.SetAttribute("Platform", EditorUserBuildSettings.activeBuildTarget.ToString());
                xmlFolder.SetAttribute("HashCode", GetFileMD5(file.FullName));
                xmlFolder.SetAttribute("Size", (file.Length / 1024f).ToString());

                xmlScene.AppendChild(xmlFolder);
            }
            root.AppendChild(xmlScene);
        }
        xmlDocument.AppendChild(root);
        xmlDocument.Save(filePath);

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
}
