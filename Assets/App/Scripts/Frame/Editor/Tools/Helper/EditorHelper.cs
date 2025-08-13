using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Editor.Helper
{
    public interface IToolkitEditor
    {
        void OnCreate(VisualElement root);
        void OnUpdate();
        void OnDestroy();
    }

    public class EditorHelper
    {
        private const string FileName = "App.Editor";
        private const string Extension = "asmdef";
        
        public const string AppConfigPath = "Assets/Resources/AppConfig.asset";
        
        public static string BaseEditorPath(string filename = FileName, string extension = Extension)
        {
            var path = AssetDatabase.FindAssets(FileName);
            return AssetDatabase.GUIDToAssetPath(path[0]).Replace("Assets/", "").Replace($"/{FileName}.{Extension}", "");
        }

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
            var directoryInfo = new DirectoryInfo(from);
            var fileInfos = directoryInfo.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension.Equals($".{extension}"))
                {
                    File.Copy($"{from}/{fileInfo.Name}", $"{to}/{fileInfo.Name}", true);
                }
            }
        }

        public static IToolkitEditor GetEditor(string scriptName)
        {
            var assembly = Assembly.Load("App.Editor");
            var type = assembly.GetType($"App.Editor.View.{scriptName}");
            var obj = Activator.CreateInstance(type); //创建此类型实例
            return obj as IToolkitEditor;
        }

        public static List<string> GetScriptName(string assemblyName, string type, bool fullname = true)
        {
            var assembly = Assembly.Load(assemblyName);
            var types = assembly.GetTypes();
            var names = new List<string>();
            foreach (var t in types)
            {
                if (t.IsDefined(typeof(CompilerGeneratedAttribute), false)) continue;
                if (t.GetInterface(type) == null) continue;
                var name = fullname ? t.FullName : t.Name;
                names.Add(name);
            }
            return names;
        }

        public static T GetEditorWindowsAsset<T>(string path) where T : ScriptableObject
        {
            return AssetDatabase.LoadAssetAtPath<T>($"Assets/{BaseEditorPath()}/Tools/Settings/Views/{path}");
        }

        public static string GetGitConfig(string key)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"config --get {key}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return result;
        }

        public static List<FileInfo> GetFiles(string basePath, string extension)
        {
            if (!Directory.Exists(basePath)) return new List<FileInfo>();
            var directoryInfo = new DirectoryInfo(basePath);
            var files = directoryInfo.GetFiles($"*.{extension}", SearchOption.AllDirectories);
            return files.ToList();
        }
    }
}