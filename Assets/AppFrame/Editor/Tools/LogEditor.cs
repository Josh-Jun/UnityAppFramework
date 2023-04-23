using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

public class LogEditor
{
    private static LogEditor m_Instance;

    public static LogEditor GetInstacne()
    {
        if (m_Instance == null)
        {
            m_Instance = new LogEditor();
        }

        return m_Instance;
    }

    private const string LogCSName = "Log";
    private const string LogCS = "Log.cs";
    private int m_DebugerFileInstanceId;
    private Type m_ConsoleWindowType = null;
    private FieldInfo m_ActiveTextInfo;
    private FieldInfo m_ConsoleWindowFileInfo;

    private LogEditor()
    {
        UnityEngine.Object debuggerFile =
            AssetDatabase.LoadAssetAtPath(GetPath(LogCSName), typeof(UnityEngine.Object));
        m_DebugerFileInstanceId = debuggerFile.GetInstanceID();
        m_ConsoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
        m_ActiveTextInfo =
            m_ConsoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
        m_ConsoleWindowFileInfo =
            m_ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
    }

    static string GetPath(string _scriptName)
    {
        string[] path = AssetDatabase.FindAssets(_scriptName);
        if (path.Length == 0)
        {
            Debug.LogError("无法找到文件" + _scriptName + "获取路径失败");
            return null;
        }

        string _path = path[0];
        if (path.Length > 1)
        {
            _path = string.Empty;
            for (var i = 0; i < path.Length; i++)
            {
                string guid2path = AssetDatabase.GUIDToAssetPath(path[i]);
                if (guid2path.EndsWith($"/{_scriptName}.cs"))
                {
                    _path = guid2path;
                    break;
                }
                // Debug.LogError("有同名文件" + _path + "获取路径失败");
            }

            if (string.IsNullOrEmpty(_path))
            {
                Debug.LogError("有同名文件" + _scriptName + "获取路径失败");
                return null;
            }
        }

        //将字符串中得脚本名字和后缀统统去除掉
        // assetPath = assetPath.Replace((@"/" + _scriptName+“.cs”), "");
        return _path;
    }

    [OnOpenAsset(0)]
    private static bool OnOpenAsset(int instanceID, int line)
    {
        string path = AssetDatabase.GetAssetPath(instanceID);

        if (instanceID == LogEditor.GetInstacne().m_DebugerFileInstanceId)
        {
            return LogEditor.GetInstacne().FindCode();
        }

        return false;
    }

    public bool FindCode()
    {
        var windowInstance = m_ConsoleWindowFileInfo.GetValue(null);
        var activeText = m_ActiveTextInfo.GetValue(windowInstance);
        string[] contentStrings = activeText.ToString().Split('\n');
        List<string> filePath = new List<string>();
        int openIndex = -1;
        for (int index = 0; index < contentStrings.Length; index++)
        {
            if (contentStrings[index].Contains("at"))
            {
                filePath.Add(contentStrings[index]);
                if (contentStrings[index].Contains(LogCS))
                {
                    openIndex = filePath.Count;
                }
            }
        }

        string fileContext = filePath[openIndex];
        bool success = PingAndOpen(fileContext);
        return success;
    }

    public bool PingAndOpen(string fileContext)
    {
        string regexRule = @"at ([\w\W]*):(\d+)\)";
        Match match = Regex.Match(fileContext, regexRule);
        if (match.Groups.Count > 1)
        {
            string path = match.Groups[1].Value;
            string line = match.Groups[2].Value;
            InternalEditorUtility.OpenFileAtLineExternal(path, int.Parse(line));
            return true;
        }

        return false;
    }
}