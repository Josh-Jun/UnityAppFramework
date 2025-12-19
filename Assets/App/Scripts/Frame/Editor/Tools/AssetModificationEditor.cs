/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年12月19 17:11
 * function    : 
 * ===============================================
 * */

using System;
using System.IO;
using System.Linq;
using System.Text;
using App.Editor.Helper;
using UnityEditor;
using UnityEngine;

namespace App.Editor.Tools
{
    public class AssetModificationEditor : AssetModificationProcessor
    {
        #region 脚本模板导入修改命名空间
        
        private static readonly StringBuilder head = new StringBuilder(1024);
        private static readonly string[] temps = { "Logic", "View" };
        /// <summary>  
        /// 此函数在asset被创建完，文件已经生成到磁盘上，但是没有生成.meta文件和import之前被调用  
        /// </summary>  
        /// <param name="newFileMeta">newFileMeta 是由创建文件的path加上.meta组成的</param>  
        public static void OnWillCreateAsset(string newFileMeta)
        {
            head.Length = 0;
            head.AppendLine("/* *");
            head.AppendLine(" * ===============================================");
            head.AppendLine($" * author      : {EditorHelper.GetGitConfig("user.name")}");
            head.AppendLine($" * e-mail      : {EditorHelper.GetGitConfig("user.email")}");
            head.AppendLine($" * create time : {DateTime.Now.Year}年{DateTime.Now.Month}月{DateTime.Now.Day} {DateTime.Now.Hour}:{DateTime.Now.Minute}");
            head.AppendLine(" * function    : ");
            head.AppendLine(" * ===============================================");
            head.AppendLine(" * */");
            
            var newFilePath = newFileMeta.Replace(".meta", "");
            if (Path.GetExtension(newFilePath) != ".txt" && Path.GetExtension(newFilePath) != ".cs") return;
            var realPath = Application.dataPath.Replace("Assets", "") + newFilePath;
            if (!File.Exists(realPath)) return;
            var scriptContent = File.ReadAllText(realPath);
            // 这里实现自定义的一些规则
            var name = Path.GetFileNameWithoutExtension(newFilePath);
            name = temps.Aggregate(name, (current, t) => current.Replace(t, ""));

            scriptContent = scriptContent.Replace("#MODULE#", name);
            scriptContent = scriptContent.Insert(0, head.ToString());

            File.WriteAllText(realPath, scriptContent);
        }
        
        #endregion
        
        #region 自动生成资源包枚举类型
        
        /// <summary>  
        /// 此函数在asset保存前被调用 
        /// </summary>  
        /// <param name="paths">paths 要保存的文件路径集合</param> 
        public static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                // 生成资源包枚举
                if (path == EditorHelper.watchers[0])
                {
                    MenuToolsEditor.UpdateAssetPackage();
                }
            }
            return paths;
        }

        #endregion
    }
}
