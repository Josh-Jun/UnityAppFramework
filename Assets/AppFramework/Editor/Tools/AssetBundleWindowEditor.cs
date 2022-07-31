using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;
/// <summary>
/// AssetBundle编辑
/// </summary>
public class AssetBundleWindowEditor : EditorWindow
{
    private GUIStyle titleStyle;

    private BuildTarget buildTarget = BuildTarget.Android;
    private string outputPath;
    private string buildPath;
    private readonly Dictionary<string, Dictionary<string, string>> sceneDic = new Dictionary<string, Dictionary<string, string>>();
    private string des = "请输入本版更新描述";
    
    public AppConfig AppConfig;//App配置表 
    private readonly string configPath = "App/AppConfig";
    
    [MenuItem("Tools/My ToolsWindow/Build AssetBundle", false, 1)]
    public static void OpenWindow()
    {
        AssetBundleWindowEditor window = GetWindow<AssetBundleWindowEditor>("BuildAssetBundleWindow");
        window.Show();
    }
    public void OnEnable()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
        
        AppConfig = Resources.Load<AppConfig>(configPath);
        
        buildPath = "Assets/Resources/AssetsFolder";
        outputPath = Application.dataPath.Replace("Assets", "AssetBundle");
    }
    private void OnGUI()
    {
        EditorGUILayout.Space();

        GUILayout.Label(new GUIContent("Build AssetBundle"), titleStyle);

        GUILayout.BeginVertical();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
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
                outputPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        //更新描述
        EditorGUILayout.Space();
        des = EditorGUILayout.TextField("Update Des", des);
        
        // AssetBundle Labels.
        EditorGUILayout.Space();
        if (GUILayout.Button("1.SetAssetBundleLabels(自动做标记)"))
        {
            RemoveAllAssetBundleLabels();
            SetAssetBundleLabels();
        }
        EditorGUILayout.Space();

        // build.
        EditorGUILayout.Space();
        if (GUILayout.Button("2.BuildAllAssetBundle(一键打包)"))
        {
            BuildAllAssetBundles();
        }

        // MD5 file.
        EditorGUILayout.Space();
        if (GUILayout.Button("3.CreateMD5File(生成MD5文件)"))
        {
            CreateFile();
        }

        // delete.
        EditorGUILayout.Space();
        if (GUILayout.Button("4.DeleteAllAssetBundle(一键删除)"))
        {
            DeleteAssetBundle();
        }
        
        // remove labels
        EditorGUILayout.Space();
        if (GUILayout.Button("5.RemoveAllAssetBundleLabels(一键清除AB包标记)"))
        {
            RemoveAllAssetBundleLabels();
            AssetDatabase.Refresh();
        }
        
        
        // remove labels
        EditorGUILayout.Space();
        if (GUILayout.Button("6.AutoBuildAssetBundle(一键自动打包)"))
        {
            RemoveAllAssetBundleLabels();
            SetAssetBundleLabels();
            DeleteAssetBundle();
            BuildAllAssetBundles();
            CreateFile();
        }
        GUILayout.EndVertical();
    }

    #region 自动做标记

    void RemoveAllAssetBundleLabels()
    {
        var names = AssetDatabase.GetAllAssetBundleNames();
        foreach (string name in names)
        {
            AssetDatabase.GetAssetPathsFromAssetBundle(name).Select(AssetImporter.GetAtPath).ToList().ForEach(x => x.assetBundleName = string.Empty);
        }
        //AssetDatabase.Refresh();
    }
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
    private void OnSceneFileSystemInfo(FileSystemInfo fileSystemInfo, string sceneName, Dictionary<string, string> namePathDic)
    {
        if (!fileSystemInfo.Exists)
            Debug.LogError(fileSystemInfo.FullName + "不存在！");
        string name = AppConfig.TargetPackage == TargetPackage.Mobile ? "Pico" : "Mobile";
        DirectoryInfo directoryInfo = fileSystemInfo as DirectoryInfo;
        // 获取所有文件系统[包括文件夹和文件]
        FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
        foreach (FileSystemInfo tempfileSystemInfo in fileSystemInfos)
        {
            if(tempfileSystemInfo.Name != name)
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
        // Debug.Log("包名为 :" + bundleName );

        // D:\Unity_forWork\Unity_Project\AssetBundle02_Senior\Assets\AssetsFolder\Scene1\Character\NB\Player4.prefab
        // 获取Assetsy以及之后的目录
        int assetIndex = fileInfo.FullName.IndexOf("Assets");
        string assetPath = fileInfo.FullName.Substring(assetIndex);
        // Debug.Log(assetPath); // Assets\AssetsFolder\Scene1\Character\NB\Player4.prefab
        string platformNmae = AppConfig.TargetPackage == TargetPackage.Mobile?"Pico":"Mobile";
        // 6.用 AssetImporter 类 ， 修改名称和后缀 ；
        AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath); // GetAtPath方法是获取Assets文件和之后的目录
        assetImporter.assetBundleName = bundleName.ToLower();
        if (fileInfo.Extension == ".unity")
            assetImporter.assetBundleVariant = "sc";
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
    #endregion

    #region 打包
    //[MenuItem("AssetBundle/BuildAllAssetBundle(一键打包)")]
    void BuildAllAssetBundles()
    {
        string outPath = string.Format("{0}/{1}", outputPath, buildTarget);
        if (!FileManager.FolderExist(outPath))
        {
            FileManager.CreateFolder(outPath);
        }

        if (BuildPipeline.BuildAssetBundles(outPath, BuildAssetBundleOptions.ChunkBasedCompression, buildTarget))
        {

            AssetDatabase.Refresh();
            Debug.Log("AssetBundle 打包成功! " + outPath);
        }
    }
    #endregion

    #region 删除
    //[MenuItem("AssetBundle/DeleteAllAssetBundle(一键删除)")]
    private void DeleteAssetBundle()
    {
        string outPath = string.Format("{0}/{1}", outputPath, buildTarget);
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
        string outPath = string.Format("{0}/{1}", outputPath, buildTarget);
        string filePath = outPath + "/AssetBundleConfig.xml";
        if (File.Exists(filePath))
            File.Delete(filePath);

        XmlDocument xmlDocument = new XmlDocument();
        XmlDeclaration xmlDeclaration = xmlDocument.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDocument.AppendChild(xmlDeclaration);
        var root = xmlDocument.CreateElement("AssetBundleConfig");
        root.SetAttribute("GameVersion", Application.version);
        root.SetAttribute("Platform", buildTarget.ToString());
        root.SetAttribute("Des", des);
        foreach (var scene in sceneDic)
        {
            var xmlScene = xmlDocument.CreateElement("Scenes");
            xmlScene.SetAttribute("SceneName", scene.Key);
            //root.AppendChild(xmlScene);
            foreach (var folder in scene.Value)
            {
                FileInfo file = new FileInfo(outPath + "/" + folder.Value);
                var xmlFolder = xmlDocument.CreateElement("Folders");
                xmlFolder.SetAttribute("FolderName", folder.Key);
                xmlFolder.SetAttribute("BundleName", folder.Value);
                xmlFolder.SetAttribute("Tag", "0");
                xmlFolder.SetAttribute("MD5", GetFileMD5(file.FullName));
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
