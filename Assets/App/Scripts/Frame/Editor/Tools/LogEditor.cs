using System;
using System.Reflection;
using UnityEditor;

namespace App.Editor.Tools
{
    public static class LogEditor
    {
        private class LogEditorConfig
        {
            public readonly string logScriptPath;
            public int instanceID = 0;

            public LogEditorConfig(string logScriptPath)
            {
                this.logScriptPath = logScriptPath;
            }
        }

        //Add your custom Log class here  
        private static readonly LogEditorConfig[] _logEditorConfig = new LogEditorConfig[]
        {
            new LogEditorConfig("Assets/App/Scripts/Frame/Core/Tools/Log/Log.cs"),
        };

        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            for (var i = _logEditorConfig.Length - 1; i >= 0; --i)
            {
                var configTmp = _logEditorConfig[i];
                UpdateLogInstanceID(configTmp);
                if (instanceID != configTmp.instanceID) continue;
                var track = GetStackTrace();
                if (!string.IsNullOrEmpty(track))
                {
                    var fileNames = track.Split('\n');
                    var fileName = GetCurrentFullFileName(fileNames);
                    var fileLine = LogFileNameToFileLine(fileName);
                    fileName = GetRealFileName(fileName);

                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileName), fileLine);
                    return true;
                }

                break;
            }

            return false;
        }

        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo =
                consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (fieldInfo == null) return "";
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null == consoleWindowInstance) return "";
            if (EditorWindow.focusedWindow != (EditorWindow)consoleWindowInstance) return "";
            // Get ListViewState in ConsoleWindow  
            // var listViewStateType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ListViewState");  
            // fieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);  
            // var listView = fieldInfo.GetValue(consoleWindowInstance);  

            // Get row in listViewState  
            // fieldInfo = listViewStateType.GetField("row", BindingFlags.Instance | BindingFlags.Public);  
            // int row = (int)fieldInfo.GetValue(listView);  

            // Get m_ActiveText in ConsoleWindow  
            fieldInfo = consoleWindowType.GetField("m_ActiveText",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (fieldInfo == null) return "";
            var activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();
            return activeText;
        }

        private static void UpdateLogInstanceID(LogEditorConfig config)
        {
            if (config.instanceID > 0)
            {
                return;
            }

            var assetLoadTmp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(config.logScriptPath);
            if (!assetLoadTmp)
            {
                throw new Exception("not find asset by path=" + config.logScriptPath);
            }

            config.instanceID = assetLoadTmp.GetInstanceID();
        }

        private static string GetCurrentFullFileName(string[] fileNames)
        {
            var retValue = "";
            var findIndex = -1;

            for (var i = fileNames.Length - 1; i >= 0; --i)
            {
                var isCustomLog = false;
                for (var j = _logEditorConfig.Length - 1; j >= 0; --j)
                {
                    if (!fileNames[i].Contains(_logEditorConfig[j].logScriptPath)) continue;
                    isCustomLog = true;
                    break;
                }

                if (!isCustomLog) continue;
                findIndex = i;
                break;
            }

            if (findIndex >= 0 && findIndex < fileNames.Length - 1)
            {
                retValue = fileNames[findIndex + 1];
            }

            return retValue;
        }

        private static string GetRealFileName(string fileName)
        {
            var indexStart = fileName.IndexOf("(at ", StringComparison.Ordinal) + "(at ".Length;
            var indexEnd = ParseFileLineStartIndex(fileName) - 1;

            fileName = fileName.Substring(indexStart, indexEnd - indexStart);
            return fileName;
        }

        private static int LogFileNameToFileLine(string fileName)
        {
            var findIndex = ParseFileLineStartIndex(fileName);
            var stringParseLine = "";
            for (var i = findIndex; i < fileName.Length; ++i)
            {
                var charCheck = fileName[i];
                if (!IsNumber(charCheck))
                {
                    break;
                }
                else
                {
                    stringParseLine += charCheck;
                }
            }

            return int.Parse(stringParseLine);
        }

        private static int ParseFileLineStartIndex(string fileName)
        {
            var retValue = -1;
            for (var i = fileName.Length - 1; i >= 0; --i)
            {
                var charCheck = fileName[i];
                var isNumber = IsNumber(charCheck);
                if (isNumber)
                {
                    retValue = i;
                }
                else
                {
                    if (retValue != -1)
                    {
                        break;
                    }
                }
            }

            return retValue;
        }

        private static bool IsNumber(char c)
        {
            return c is >= '0' and <= '9';
        }
    }
}