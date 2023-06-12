using System.IO;
using System.Text;
using UnityEngine;

public class HotfixTool
{
    public static void CreateFile(string targetPath, byte[] bytes)
    {
        try
        {
            CreateDirectory(targetPath);
            DeleteFile(targetPath);
            FileInfo file = new FileInfo(targetPath);
            Stream stream = file.Create();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 创建文件-文件夹路径错误:{0}</color>", targetPath));
        }
    }
    /// <summary> 创建文件 </summary>
    public static void CreateFile(string targetPath, string content)
    {
        try
        {
            CreateDirectory(targetPath);
            DeleteFile(targetPath);
            FileInfo file = new FileInfo(targetPath);
            Stream stream = file.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 创建文件-文件夹路径错误:{0}</color>", targetPath));
        }
    }
    /// <summary> 创建目录,创建到路径最后一级的文件夹 </summary>
    public static void CreateDirectory(string targetPath)
    {
        try
        {
            string dirName = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 创建目录-文件夹路径错误:{0}</color>", targetPath));
        }
    }
    /// <summary> 删除文件 </summary>
    public static void DeleteFile(string targetPath)
    {
        try
        {
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF>删除文件-文件夹路径错误:{0}</color>", targetPath));
        }
    }
    /// <summary> 文件是否存在 </summary>
    public static bool FileExist(string targetPath)
    {
        return File.Exists(targetPath);
    }
}