using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
    public class FindSameFileName : IToolkitEditor
    {
        private string[] items =
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
        private Dictionary<string, string> FileNames = new Dictionary<string, string>();
        public void OnCreate(VisualElement root)
        {
            var objectType = root.Q<EnumField>("object_type");
            objectType.Init(ObjectType.All);
            var textFilesField = root.Q<TextField>("text_files_path");
            textFilesField.value = "";

            root.Q<Button>("files_path_browse").clicked += () => { textFilesField.value = EditorTool.Browse(); };

            root.Q<Button>("btn_find").clicked += () =>
            {
                FindSameFile((ObjectType)objectType.value, textFilesField.value);
            };
        }
        public void OnUpdate()
        {
            
        }
        public void OnDestroy()
        {
            
        }

        private void FindSameFile(ObjectType objectType, string filesPath)
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
