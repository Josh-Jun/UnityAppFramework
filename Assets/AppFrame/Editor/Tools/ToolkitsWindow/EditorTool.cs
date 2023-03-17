using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AppFrame.Editor
{
    public class EditorTool
    {
        public static string Browse(bool isFullPath = false)
        {
            var newPath = EditorUtility.OpenFolderPanel("Prefabs Folder", Application.dataPath, string.Empty);
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
    }
}
