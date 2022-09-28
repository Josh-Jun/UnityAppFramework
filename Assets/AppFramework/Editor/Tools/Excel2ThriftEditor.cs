using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Excel2Thrift;
using Thrift;
using System;

public class Excel2ThriftEditor : EditorWindow
{

    static int m_Selected = 0;
    static List<bool> m_ExportList = new List<bool>();
    static List<string> m_DataList = new List<string>();

    static bool m_ExportCode = true;
    static bool m_ExportTab = false;
    static bool m_ExportExecuteCode = false;
    Vector2 scrollPosition;

    private static Excel2Thrift.ExecuteParam m_Parm = new ExecuteParam();
    private static string m_BytePath = "";
    private static string m_TablePath = "";
    private static string m_FileListPath = "";

    [MenuItem("Tools/My ToolsWindow/Excel2Thrift", false, 0)]
    public static void OpenWindow()
    {
        Excel2ThriftEditor window = GetWindow<Excel2ThriftEditor>("Export Thrift");
        window.Show();
    }


    private void SetDataAndPath()
    {
        string dataPath = Application.dataPath.Replace("/Assets", "/Data/");
        m_Parm.thriftFile = dataPath + "thrift/meta.thrift";
        m_Parm.csPath = dataPath + "thrift/output/";
        m_Parm.bytePath = dataPath + "thrift/output/";
        m_Parm.epplusLibFile = Application.dataPath + "/AppFramework/Editor/Plugins/EPPlus.dll";
        m_Parm.thriftLibFile = Application.dataPath + "/AppFramework/Libraries/Thrift/Thrift.dll";

        m_BytePath = Application.dataPath + "/Resources/AssetsFolder/Table/Thrift/";
        m_TablePath = Application.dataPath + "/Scripts/Table/";
    }

    private void InitData()
    {
        m_ExportList.Clear();
        m_DataList.Clear();
        m_Selected = 0;

        SetDataAndPath();
    }

    public void OnEnable()
    {
        InitData();
        LoadFiles();
    }
    public void OnGUI()
    {
        EditorGUILayout.Space();
      

        EditorGUILayout.BeginHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(240));//注意：scrollPosition 必须要有，不然会拖动不了
        {
            for (int i = 0; i < m_DataList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                m_ExportList[i] = EditorGUILayout.Toggle(m_ExportList[i], GUILayout.MaxWidth(32));
                GUILayout.Label(m_DataList[i]);
                EditorGUILayout.EndHorizontal();
            }
        }
        GUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6f);

        EditorGUILayout.BeginHorizontal();
        m_Selected = 0;
        for (int i=0; i<m_ExportList.Count; i++)
        {
            if (m_ExportList[i])
                m_Selected++;
        }
        GUILayout.Label($"{m_Selected}/{m_DataList.Count}", GUILayout.MaxWidth(40f));
        GUILayout.Label("files selected", GUILayout.MaxWidth(100f));  
        EditorGUILayout.EndHorizontal();
        GUILayout.Button("", GUILayout.Height(1));
        GUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
               
        if (GUILayout.Button("Mark-All", GUILayout.MaxWidth(80f)))
        {
            for (int i = 0; i < m_ExportList.Count; i++)
                m_ExportList[i] = true;
        }
        GUILayout.Space(12);
        if (GUILayout.Button("UnMark-All", GUILayout.MaxWidth(80f)))
        {
            for (int i = 0; i < m_ExportList.Count; i++)
                m_ExportList[i] = false;
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reload", GUILayout.MaxWidth(80f)))
        {
            LoadFiles();
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(10f);
        if (GUILayout.Button("Export"))
        {
            OnExport();
        }
    }

    private static string m_ExcelPath = "";
    private static void LoadFiles()
    {
        m_ExcelPath = Application.dataPath;
        m_ExcelPath = m_ExcelPath.Replace("/Assets", "/Data/excel/");

        GetFiles(m_ExcelPath, ".xlsx");
    }

    private static void GetFiles(string dirPath, string file_type)
    {
        m_DataList.Clear();
        m_ExportList.Clear();
        foreach (string path in Directory.GetFiles(dirPath))
        {
            if (System.IO.Path.GetExtension(path) == file_type)
            {
                m_DataList.Add(System.IO.Path.GetFileName(path));
                m_ExportList.Add(true);
            }
        }
    }

    private static void OnExport()
    {
        ClearFolder(m_Parm.csPath, ".cs");
        ClearFolder(m_Parm.bytePath, ".bytes");
        ExecuteConvert();
        CopyToProject();
    }

    private static void ExecuteConvert()
    {
        if (m_DataList.Count == 0 || m_Selected == 0)
            return;

        List<string> excelFiles = new List<string>();
        for (int i = 0; i < m_DataList.Count; i++)
        {
            if (m_ExportList[i])
                excelFiles.Add(m_ExcelPath + m_DataList[i]);
        }


        if (!Directory.Exists(m_Parm.csPath))
        {
            Directory.CreateDirectory(m_Parm.csPath);
        }

        string ret = Excel2Thrift.Convert.Execute(m_Parm, excelFiles, m_ExportTab, m_ExportExecuteCode, m_ExportCode);
        Debug.Log(ret);
    }

    private static Dictionary<string, string> m_NamePathDic = new Dictionary<string, string>();
    private static void CreateFileList(string desPath, string desFile)
    {
        if (!Directory.Exists(desPath))
            Directory.CreateDirectory(desPath);

        StringBuilder sb = new StringBuilder(2048);
        foreach(var kv in m_NamePathDic)
        {
            sb.Append(kv.Key);
            sb.Append("\r\n");
            sb.Append(kv.Value);
            sb.Append("\r\n");
        }

        File.WriteAllText(desPath+desFile, sb.ToString());
    }

    private static void CopyToProject()
    {
        m_NamePathDic.Clear();

        foreach (string path in Directory.GetFiles(m_Parm.bytePath))
        {
            if (System.IO.Path.GetExtension(path) == ".bytes")
            {
                string desFilePath = m_BytePath + System.IO.Path.GetFileName(path);
                if (File.Exists(desFilePath))
                    File.Delete(desFilePath);

                File.Copy(path, desFilePath);

                string fileName1 = Path.GetFileNameWithoutExtension(path);
                string path1 = $"Table/Thrift/{fileName1}";
                if ( !m_NamePathDic.ContainsKey( fileName1))
                {
                    m_NamePathDic.Add(fileName1, path1);
                }             
                
            }
        }       

        foreach (string path in Directory.GetFiles(m_Parm.csPath))
        {
            if (System.IO.Path.GetExtension(path) == ".cs")
            {
                string desFile = m_TablePath + System.IO.Path.GetFileName(path);
                if (File.Exists(desFile))
                    File.Delete(desFile);

                File.Copy(path, m_TablePath + System.IO.Path.GetFileName(path));
            }
        }

        AssetDatabase.Refresh();
    }



    private static void ClearFolder(string folderPath, string fileType)
    {
        if (!Directory.Exists(folderPath))
            return;
        foreach (string path in Directory.GetFiles(folderPath))
        {
            if (System.IO.Path.GetExtension(path) == fileType)
            {
                File.Delete(path);
            }
        }
    }

}
