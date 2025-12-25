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

    public static class EditorHelper
    {
        private const string FileName = "App.Editor";
        private const string Extension = "asmdef";

        public const string AppConfigPath = "Assets/Resources/AppConfig.asset";
        public const int MENU_LEVEL = 1;
        
        public static readonly string[] watchers = new[]
        {
            "Assets/Settings/AssetBundleCollectorSetting.asset"
        };

        public static string BaseEditorPath(string filename = FileName, string extension = Extension)
        {
            var path = AssetDatabase.FindAssets(FileName);
            return AssetDatabase.GUIDToAssetPath(path[0]).Replace("Assets/", "")
                .Replace($"/{FileName}.{Extension}", "");
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
        
        public static List<Type> GetAssemblyTypes<T>(string assemblyString = "App.Module")
        {
            var assembly = Assembly.Load(assemblyString);
            var types = assembly.GetTypes();
            return types.Where(type => type.Name != typeof(T).Name && typeof(T).IsAssignableFrom(type)).ToList();
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

        #region GameViewResolution

        private const int DefaultOrientation = 0;
        private const int DefaultWidth = 1170;
        private const int DefaultHeight = 2532;
        private static int CurrentOrientation = -1;

        public static void ChangeGameViewResolution(int orientation)
        {
            if (CurrentOrientation == orientation) return;
            CurrentOrientation = orientation;
            var width = orientation == 0 ? DefaultWidth : DefaultHeight;
            var height = orientation == 0 ? DefaultHeight : DefaultWidth;
            SetGameViewSize(width, height);
        }
        
        public static void SwitchGameViewResolution()
        {
            var orientation = CurrentOrientation > 0 ? 0 : 1;
            ChangeGameViewResolution(orientation);
        }

        public static void RestoreGameViewResolution()
        {
            ChangeGameViewResolution(DefaultOrientation);
        }

        public static void ClearGameViewCustomSize()
        {
            // 获取 GameViewSizes 单例实例
            var gameViewSizesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
            var scriptableSingletonType = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
            var instanceProp = scriptableSingletonType.GetProperty("instance");
            var gameViewSizesInstance = instanceProp?.GetValue(null, null);

            // 获取当前分组
            var currentGroupProp = gameViewSizesInstance?.GetType().GetProperty("currentGroup");
            var currentGroup = currentGroupProp?.GetValue(gameViewSizesInstance, null);

            // 获取所有尺寸
            var getTotalCount = currentGroup?.GetType().GetMethod("GetTotalCount");
            var getBuiltinCount = currentGroup?.GetType().GetMethod("GetBuiltinCount");
            var totalCount = (int)getTotalCount?.Invoke(currentGroup, null)!;
            var builtinCount = (int)getBuiltinCount!.Invoke(currentGroup, null);

            // 反向遍历并删除自定义尺寸
            for (var i = totalCount - 1; i >= builtinCount; i--)
            {
                var getGameViewSize = currentGroup.GetType().GetMethod("GetGameViewSize");
                var size = getGameViewSize?.Invoke(currentGroup, new object[] { i });

                var sizeType = size?.GetType().GetProperty("sizeType");
                var typeValue = (int)sizeType?.GetValue(size, null)!;

                // 类型1是自定义分辨率
                if (typeValue != 1) continue;
                var removeCustomSize = currentGroup.GetType().GetMethod("RemoveCustomSize");
                removeCustomSize?.Invoke(currentGroup, new object[] { i });
            }
        }

        public static void AddGameViewCustomSize(int width, int height, string displayName)
        {
            // 获取 GameViewSizes 单例实例
            var gameViewSizesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
            var scriptableSingletonType = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
            var instanceProp = scriptableSingletonType.GetProperty("instance");
            var gameViewSizesInstance = instanceProp?.GetValue(null, null);

            // 获取当前分组
            var currentGroupProp = gameViewSizesInstance?.GetType().GetProperty("currentGroup");
            var currentGroup = currentGroupProp?.GetValue(gameViewSizesInstance, null);

            // 创建新的 GameViewSize
            var gameViewSizeType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSize");
            var sizeTypeEnum = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizeType");
            var ctor = gameViewSizeType.GetConstructor(
                new Type[] { sizeTypeEnum, typeof(int), typeof(int), typeof(string) });

            // 1 表示自定义分辨率类型
            var newSize = ctor?.Invoke(new object[] { 1, width, height, displayName });

            // 添加新尺寸
            var addCustomSize = currentGroup?.GetType().GetMethod("AddCustomSize");
            addCustomSize?.Invoke(currentGroup, new object[] { newSize });
            EditorUtility.RequestScriptReload();
        }

        public static void SetGameViewSize(int width, int height)
        {
            var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(gameViewType);

            // 查找匹配的分辨率索引
            var gameViewSizesInstance = typeof(ScriptableSingleton<>)
                .MakeGenericType(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes"))
                .GetProperty("instance")!
                .GetValue(null, null);

            var currentGroup = gameViewSizesInstance.GetType()
                .GetProperty("currentGroup")!
                .GetValue(gameViewSizesInstance);

            var getTotalCount = currentGroup.GetType().GetMethod("GetTotalCount");
            var totalCount = (int)getTotalCount?.Invoke(currentGroup, null)!;
            var index = -1;
            for (var i = 0; i < totalCount; i++)
            {
                var getGameViewSize = currentGroup.GetType().GetMethod("GetGameViewSize");
                var gameViewSize = getGameViewSize?.Invoke(currentGroup, new object[] { i });

                var sizeWidth = (int)gameViewSize?.GetType().GetProperty("width")?.GetValue(gameViewSize)!;
                var sizeHeight = (int)gameViewSize.GetType().GetProperty("height")?.GetValue(gameViewSize)!;

                if (sizeWidth != width || sizeHeight != height) continue;
                // 找到匹配的分辨率
                index = i;
                break;
            }

            if (index == -1)
            {
                var str = width > height ? "Landscape" : "Portrait";
                AddGameViewCustomSize(width, height, $"{width}x{height} {str}");
            }

            var setSizeMethod = gameViewType.GetMethod("SizeSelectionCallback");
            setSizeMethod?.Invoke(gameView, new object[] { index, null });
        }

        #endregion
    }
}