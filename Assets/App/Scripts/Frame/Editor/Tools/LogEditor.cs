using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;

namespace App.Editor.Tools
{
    public class LogEditor
    {
        private static LogEditor m_Instance;

        private static LogEditor GetInstacne()
        {
            return m_Instance ??= new LogEditor();
        }

        private const string LogCSName = "Log";
        private const string LogCS = "Log.cs";
        private readonly int m_DebugerFileInstanceId;
        private readonly FieldInfo m_ActiveTextInfo;
        private readonly FieldInfo m_ConsoleWindowFileInfo;

        private LogEditor()
        {
            var debuggerFile =
                AssetDatabase.LoadAssetAtPath(GetPath(LogCSName), typeof(UnityEngine.Object));
            m_DebugerFileInstanceId = debuggerFile.GetInstanceID();
            var mConsoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
            if (mConsoleWindowType != null)
            {
                m_ActiveTextInfo =
                    mConsoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                m_ConsoleWindowFileInfo =
                    mConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        private static string GetPath(string _scriptName)
        {
            var path = AssetDatabase.FindAssets(_scriptName);
            if (path.Length == 0)
            {
                Debug.LogError("无法找到文件" + _scriptName + "获取路径失败");
                return null;
            }

            var _path = path[0];
            if (path.Length > 1)
            {
                _path = string.Empty;
                foreach (var t in path)
                {
                    var guid2path = AssetDatabase.GUIDToAssetPath(t);
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
            var path = AssetDatabase.GetAssetPath(instanceID);

            if (instanceID == LogEditor.GetInstacne().m_DebugerFileInstanceId)
            {
                return LogEditor.GetInstacne().FindCode();
            }

            return false;
        }

        private bool FindCode()
        {
            var windowInstance = m_ConsoleWindowFileInfo.GetValue(null);
            var activeText = m_ActiveTextInfo.GetValue(windowInstance);
            var contentStrings = activeText.ToString().Split('\n');
            var filePath = new List<string>();
            var openIndex = -1;
            foreach (var t in contentStrings)
            {
                if (t.Contains("at"))
                {
                    filePath.Add(t);
                    if (t.Contains(LogCS))
                    {
                        openIndex = filePath.Count;
                    }
                }
            }

            var fileContext = filePath[openIndex];
            var success = PingAndOpen(fileContext);
            return success;
        }

        private bool PingAndOpen(string fileContext)
        {
            var regexRule = @"at ([\w\W]*):(\d+)\)";
            var match = Regex.Match(fileContext, regexRule);
            if (match.Groups.Count > 1)
            {
                var path = match.Groups[1].Value;
                var line = match.Groups[2].Value;
                InternalEditorUtility.OpenFileAtLineExternal(path, int.Parse(line));
                return true;
            }

            return false;
        }
    }
}