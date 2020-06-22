using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class MyToolsWindowEditor : EditorWindow
{
    //private static Texture texture;
    private static GUIStyle titleStyle;

    #region 1、更换预制体字体
    private string prefabsPath = "";

    private Font changeFont;

    #endregion

    #region 2、查找同名文件
    private enum ObjType
    {
        All = 0,
        Prefab = 1,
        Texture = 2,
        AudioClip = 3,
        VideoClip = 4,
        Scene = 5,
        Material = 6,
        Model = 7,
        AnimationClip = 8,
        Shader = 9,
        TextAsset = 10,
        AnimatorController = 11,
        PhysicMaterial = 12,
        PhysicsMaterial2D = 13
    }

    private string[] items = {
        "t:Prefab t:Texture t:AudioClip t:Scene t:Material t:Model t:AnimationClip t:Shader t:TextAsset t:AnimatorController t:PhysicMaterial t:PhysicsMaterial2D",
        "t:Prefab",
        "t:Texture",
        "t:AudioClip",
        "t:VideoClip",
        "t:Scene",
        "t:Material",
        "t:Model",
        "t:AnimationClip",
        "t:Shader",
        "t:TextAsset",
        "t:AnimatorController",
        "t:PhysicMaterial",
        "t:PhysicsMaterial2D" };

    private ObjType objType = ObjType.All;

    private static Dictionary<string, string> FileNames = new Dictionary<string, string>();

    private string filesPath = "";
    #endregion

    #region 3、代码打包Dll
    private string dllOutPath = "";

    private string dllsPath = "";

    private string scriptsPath = "";
    #endregion

    [MenuItem("Tools/My ToolsWindow")]
    public static void OpenWindow()
    {
        //texture = AssetDatabase.LoadAssetAtPath("Assets/Editor/Tools/Image/bg.png", typeof(Texture)) as Texture;
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        MyToolsWindowEditor window = GetWindow<MyToolsWindowEditor>("MyToolsWindow");
        window.Show();
    }

    [System.Obsolete]
    private void OnGUI()
    {
        #region 1、更换预制体字体
        EditorGUILayout.Space();

        GUILayout.Label(new GUIContent("Change Prefabs Font"), titleStyle);

        EditorGUILayout.Space();

        GUILayout.BeginVertical();

        changeFont = (Font)EditorGUILayout.ObjectField(changeFont, typeof(Font), true);

        EditorGUILayout.Space();

        prefabsPath = EditorGUILayout.TextField("Prefabs Path", prefabsPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Prefabs Folder", prefabsPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                prefabsPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Change Prefabs Font(更换字体)"))
        {
            EditorApplication.delayCall += ChangePrefabsFont;
        }

        GUILayout.EndVertical();
        #endregion

        #region 2、查找同名文件
        GUILayout.Label(new GUIContent("Find Same File Name"), titleStyle);

        EditorGUILayout.Space();

        GUILayout.BeginVertical();

        objType = (ObjType)EditorGUILayout.EnumPopup("ObjectType", objType);

        EditorGUILayout.Space();

        filesPath = EditorGUILayout.TextField("Files Path", filesPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Files Folder", prefabsPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                filesPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Find Same File Name(查找同名文件)"))
        {

            EditorApplication.delayCall += FindSameFileName;
        }

        GUILayout.EndVertical();
        #endregion

        #region 3、代码打包Dll
        GUILayout.Label(new GUIContent("Build Dll Package"), titleStyle);

        EditorGUILayout.Space();

        GUILayout.BeginVertical();

        dllsPath = EditorGUILayout.TextField("Dlls Path", dllsPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Dlls Folder", prefabsPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                dllsPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        scriptsPath = EditorGUILayout.TextField("Scripts Path", scriptsPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Scripts Folder", prefabsPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                scriptsPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        dllOutPath = EditorGUILayout.TextField("DllOutPath Path", dllOutPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Output Folder", prefabsPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                dllOutPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Build Dll Package(打Dll文件包)"))
        {

            EditorApplication.delayCall += BuildDllPackage;
        }

        GUILayout.EndVertical();
        #endregion
    }

    #region 1、更换预制体字体
    private void ChangePrefabsFont()
    {
        if (changeFont == null)
        {
            Debug.Log("请选择要更换的字体");
            return;
        }
        string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { prefabsPath });
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                SetAllTextFont(obj.transform);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("更换完成");
    }
    private void SetAllTextFont(Transform go)
    {
        foreach (Transform item in go)
        {
            Text t = item.GetComponent<Text>();
            if (t != null)
            {
                t.font = changeFont;
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
            }
            if (item.childCount > 0)
            {
                SetAllTextFont(item);
            }
        }
    }
    #endregion

    #region 2、查找同名文件
    private void FindSameFileName()
    {
        string filter = items[(int)objType];
        string[] allPath = AssetDatabase.FindAssets(filter, new string[] { filesPath });
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            if (path.Contains("Plugins")) continue;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (!FileNames.ContainsKey(obj.name))
            {
                FileNames.Add(obj.name, path);
            }
            else
            {
                Debug.LogFormat("重复文件:{0}\n路径:{1}    {2}", obj.name, path, FileNames[obj.name]);
            }
        }
        Debug.Log("查找完了");
        FileNames.Clear();
    }
    #endregion

    #region 3、代码打包Dll
    [System.Obsolete]
    private void BuildDllPackage()
    {
        ICodeCompiler complier = new CSharpCodeProvider().CreateCompiler();
        //设置编译参数
        CompilerParameters paras = new CompilerParameters();
        //引入第三方dll
        //paras.ReferencedAssemblies.Add(@"Microsoft.CSharp.dll");
        paras.ReferencedAssemblies.Add(@"mscorlib.dll");
        paras.ReferencedAssemblies.Add(@"System.dll");
        paras.ReferencedAssemblies.Add(@"System.Core.dll");
        //paras.ReferencedAssemblies.Add(@"System.Data.dll");
        //paras.ReferencedAssemblies.Add(@"System.Data.DataSetExtensions.dll");
        //paras.ReferencedAssemblies.Add(@"System.Net.Http.dll");
        paras.ReferencedAssemblies.Add(@"System.Xml.dll");
        //paras.ReferencedAssemblies.Add(@"System.Xml.Linq.dll");


        List<FileInfo> fileList = new List<FileInfo>();
        DirectoryInfo directoryInfo = new DirectoryInfo(dllsPath);
        ListFiles(directoryInfo, ref fileList);

        //引入自定义dll
        for (int i = 0; i < fileList.Count; i++)
        {
            if (fileList[i].Extension == ".dll")
            {
                paras.ReferencedAssemblies.Add(fileList[i].FullName);
            }
        }

        //是否内存中生成输出
        paras.GenerateInMemory = false;
        //是否生成可执行文件
        paras.GenerateExecutable = false;
        paras.OutputAssembly = dllOutPath + "/HotFix.dll.bytes";

        //引入脚本
        string[] allPath = AssetDatabase.FindAssets("t:Script", new string[] { scriptsPath });
        List<string> allScript = new List<string>();
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            allScript.Add(path);
        }

        //编译代码
        CompilerResults result = complier.CompileAssemblyFromFileBatch(paras, allScript.ToArray());

        for (int i = 0; i < result.Errors.Count; i++)
        {
            Debug.Log(result.Errors[i]);
        }
        AssetDatabase.Refresh();
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
    #endregion
}
