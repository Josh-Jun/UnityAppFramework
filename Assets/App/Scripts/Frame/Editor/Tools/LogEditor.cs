using System.Reflection;
using UnityEditor;

namespace App.Editor.Tools
{
    public static class LogEditor
    {
        private class LogEditorConfig
        {
            public string logScriptPath = "";
            public int instanceID = 0;

            public LogEditorConfig(string logScriptPath)
            {
                this.logScriptPath = logScriptPath;
            }
        }

        //Add your custom Log class here  
        private static LogEditorConfig[] _logEditorConfig = new LogEditorConfig[]
        {
            new LogEditorConfig("Assets/App/Scripts/Frame/Core/Tools/Log/Log.cs"),
        };

        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            for (int i = _logEditorConfig.Length - 1; i >= 0; --i)
            {
                var configTmp = _logEditorConfig[i];
                UpdateLogInstanceID(configTmp);
                if (instanceID == configTmp.instanceID)
                {
                    var statckTrack = GetStackTrace();
                    if (!string.IsNullOrEmpty(statckTrack))
                    {
                        var fileNames = statckTrack.Split('\n');
                        var fileName = GetCurrentFullFileName(fileNames);
                        var fileLine = LogFileNameToFileLine(fileName);
                        fileName = GetRealFileName(fileName);

                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileName), fileLine);
                        return true;
                    }

                    break;
                }
            }

            return false;
        }

        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo =
                consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null != consoleWindowInstance)
            {
                if ((object)EditorWindow.focusedWindow == consoleWindowInstance)
                {
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
                    string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();

                    return activeText;
                }
            }

            return "";
        }

        private static void UpdateLogInstanceID(LogEditorConfig config)
        {
            if (config.instanceID > 0)
            {
                return;
            }

            var assetLoadTmp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(config.logScriptPath);
            if (null == assetLoadTmp)
            {
                throw new System.Exception("not find asset by path=" + config.logScriptPath);
            }

            config.instanceID = assetLoadTmp.GetInstanceID();
        }

        private static string GetCurrentFullFileName(string[] fileNames)
        {
            string retValue = "";
            int findIndex = -1;

            for (int i = fileNames.Length - 1; i >= 0; --i)
            {
                bool isCustomLog = false;
                for (int j = _logEditorConfig.Length - 1; j >= 0; --j)
                {
                    if (fileNames[i].Contains(_logEditorConfig[j].logScriptPath))
                    {
                        isCustomLog = true;
                        break;
                    }
                }

                if (isCustomLog)
                {
                    findIndex = i;
                    break;
                }
            }

            if (findIndex >= 0 && findIndex < fileNames.Length - 1)
            {
                retValue = fileNames[findIndex + 1];
            }

            return retValue;
        }

        private static string GetRealFileName(string fileName)
        {
            int indexStart = fileName.IndexOf("(at ") + "(at ".Length;
            int indexEnd = ParseFileLineStartIndex(fileName) - 1;

            fileName = fileName.Substring(indexStart, indexEnd - indexStart);
            return fileName;
        }

        private static int LogFileNameToFileLine(string fileName)
        {
            int findIndex = ParseFileLineStartIndex(fileName);
            string stringParseLine = "";
            for (int i = findIndex; i < fileName.Length; ++i)
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
            int retValue = -1;
            for (int i = fileName.Length - 1; i >= 0; --i)
            {
                var charCheck = fileName[i];
                bool isNumber = IsNumber(charCheck);
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
            return c >= '0' && c <= '9';
        }
    }
}