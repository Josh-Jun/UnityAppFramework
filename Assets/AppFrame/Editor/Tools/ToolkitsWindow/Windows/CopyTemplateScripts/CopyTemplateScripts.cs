using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public class CopyTemplateScripts : IToolkitEditor
    {
        public void OnCreate(VisualElement root)
        {
            var unity_install_path = root.Q<TextField>("unity_install_path");
            unity_install_path.value = AppDomain.CurrentDomain.BaseDirectory;
            root.Q<Button>("unity_install_path_browse").clicked += () =>
            {
                unity_install_path.value = EditorTool.Browse(true);
            };
            root.Q<Button>("btn_copy_template").clicked += () =>
            {
                Copy(unity_install_path.value);
            };
        }
        private void Copy(string copyToPath)
        {
            if (string.IsNullOrEmpty(copyToPath))
            {
                ToolkitsWindow.ShowHelpBox("请选择Unity安装目录");
                return;
            }
            var template_path = $"{Application.dataPath}/AppFrame/Editor/ScriptTemplates";
            var install_path = $"{copyToPath}/Data/Resources/ScriptTemplates";

            EditorTool.Copy(template_path, install_path, "txt");
                
            ToolkitsWindow.ShowHelpBox("拷贝完成，请重启Unity");
        }
    }
}
