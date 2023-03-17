using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AppFrame.Editor
{
    public class CopyTemplateScripts
    {
        public static void Copy(string copyToPath)
        {
            if (string.IsNullOrEmpty(copyToPath))
            {
                ToolkitsWindow.ShowHelpBox("请选择Unity安装目录");
                return;
            }
            var template_path = $"{Application.dataPath}/AppFrame/Editor/ScriptTemplates";
            var install_path = $"{copyToPath}/Data/Resources/ScriptTemplates";

            ToolkitsWindow.Copy(template_path, install_path, "txt");
                
            ToolkitsWindow.ShowHelpBox("拷贝完成，请重启Unity");
        }
    }
}
