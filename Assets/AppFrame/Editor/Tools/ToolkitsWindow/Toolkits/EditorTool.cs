using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public interface IToolkitEditor
    {
        void OnCreate(VisualElement root);
        void OnUpdate();
        void OnDestroy();
    }

    public class EditorTool
    {
        public const string BaseWindowPath = "AppFrame/Editor/Tools/ToolkitsWindow/Windows";

        public static string Browse(bool isFullPath = false)
        {
            var newPath = EditorUtility.OpenFolderPanel("Browse Folder", Application.dataPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                if (!isFullPath)
                {
                    var gamePath = System.IO.Path.GetFullPath(".");
                    gamePath = gamePath.Replace("\\", "/");
                    if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                        newPath = newPath.Remove(0, gamePath.Length + 1);
                }
            }

            return newPath;
        }

        public static void Copy(string from, string to, string extension)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(from);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Extension.Equals($".{extension}"))
                {
                    File.Copy($"{from}/{fileInfo.Name}", $"{to}/{fileInfo.Name}", true);
                }
            }
        }

        public static IToolkitEditor GetEditor(string scriptName)
        {
            Assembly assembly = Assembly.Load("App.Frame.Editor");
            Type type = assembly.GetType($"AppFrame.Editor.{scriptName}");
            var obj = Activator.CreateInstance(type); //创建此类型实例
            return obj as IToolkitEditor;
        }

        public static List<string> GetScriptName(string assemblyName, string type, bool fullname = true)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            var types = assembly.GetTypes();
            var names = new List<string>();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsDefined(typeof(CompilerGeneratedAttribute), false)) continue;
                if (types[i].GetInterface(type) == null) continue;
                var name = fullname ? types[i].FullName : types[i].Name;
                names.Add(name);
                Debug.Log(name);
            }
            return names;
        }

        public static T GetEditorWindowsAsset<T>(string path) where T : ScriptableObject
        {
            return AssetDatabase.LoadAssetAtPath<T>($"Assets/{BaseWindowPath}/{path}");
        }
    }
}