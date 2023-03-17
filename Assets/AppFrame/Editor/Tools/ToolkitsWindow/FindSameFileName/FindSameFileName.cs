using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AppFrame.Editor
{
    public enum ObjectType
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
    public class FindSameFileName
    {
        private static string[] items =
        {
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
            "t:PhysicsMaterial2D"
        };
        private static Dictionary<string, string> FileNames = new Dictionary<string, string>();
        
        public static void FindSameFile(ObjectType objectType, string filesPath)
        {
            if (string.IsNullOrEmpty(filesPath))
            {
                ToolkitsWindow.ShowHelpBox("请选择想要查找的目录");
                return;
            }
            string filter = items[(int)objectType];
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
                    ToolkitsWindow.ShowHelpBox($"重复文件:{obj.name}\n{path}    {FileNames[obj.name]}");
                }
            }

            FileNames.Clear();
            ToolkitsWindow.ShowHelpBox("查找完了");
        }
    }
}
