using UnityEditor;
using UnityEngine;
using System.IO;

public class AddScriptTemplatesEditor : UnityEditor.AssetModificationProcessor
{
    private static string[] temps = { "Root", "Window" };

    /// <summary>  
    /// 此函数在asset被创建完，文件已经生成到磁盘上，但是没有生成.meta文件和import之前被调用  
    /// </summary>  
    /// <param name="newFileMeta">newfilemeta 是由创建文件的path加上.meta组成的</param>  
    public static void OnWillCreateAsset(string newFileMeta)
    {
        var newFilePath = newFileMeta.Replace(".meta", "");
        if (Path.GetExtension(newFilePath) == ".txt" || Path.GetExtension(newFilePath) == ".cs")
        {
            var realPath = Application.dataPath.Replace("Assets", "") + newFilePath;
            var scriptContent = File.ReadAllText(realPath);
            // 这里实现自定义的一些规则
            // scriptContent = scriptContent.Replace("#SCRIPTNAME#", Path.GetFileNameWithoutExtension(newFilePath));
            var namespaces = Path.GetFileNameWithoutExtension(newFilePath);
            for (int i = 0; i < temps.Length; i++)
            {
                namespaces.Replace(temps[i], "");
            }

            scriptContent = scriptContent.Replace("#NAMESPACE#", namespaces);

            File.WriteAllText(realPath, scriptContent);
        }
    }
}