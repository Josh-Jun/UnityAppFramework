using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 功能:文件工具 
/// </summary>
public class FileManager
{
    /// <summary> 创建文件 </summary>
    public static void CreateFile(string targetPath, byte[] bytes)
    {
        string path = targetPath.Replace("file://","");
        try
        {
            CreateDirectory(path);
            DeleteFile(path);
            FileInfo file = new FileInfo(path);
            Stream stream = file.Create();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            stream.Dispose();
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 创建文件-文件夹路径错误:{0}</color>", path));
        }
    }

    /// <summary> 创建文件 </summary>
    public static void CreateFile(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        try
        {
            CreateDirectory(path);
            DeleteFile(path);
            FileInfo file = new FileInfo(path);
            Stream stream = file.Create();
            stream.Close();
            stream.Dispose();
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 创建文件-文件夹路径错误:{0}</color>", path));
        }
    }
    /// <summary> 创建目录,创建到路径最后一级的文件夹 </summary>
    public static void CreateDirectory(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        try
        {
            string dirName = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 创建目录-文件夹路径错误:{0}</color>", path));
        }
    }

    /// <summary> 创建文件夹 </summary>
    public static void CreateFolder(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF>创建文件夹-文件夹路径错误:{0}</color>", path));
        }
    }

    /// <summary> 删除文件 </summary>
    public static void DeleteFile(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF>删除文件-文件夹路径错误:{0}</color>", path));
        }
    }

    /// <summary> 删除文件夹下所有文件 </summary>
    public static void DeleteFolderAllFile(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        try
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            FileSystemInfo[] files = folder.GetFileSystemInfos();
            int length = files.Length;
            for (int i = 0; i < length; i++)
            {
                //如果这个文件是文件夹
                if (files[i] is DirectoryInfo)
                {
                    DirectoryInfo subdir = new DirectoryInfo(files[i].FullName);
                    subdir.Delete(true);          //删除子目录和文件
                }
                else
                {
                    File.Delete(files[i].FullName); //删除文件
                }
            }
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 删除文件夹下所有文件-文件夹路径错误:{0}</color>", path));
        }
    }

    /// <summary> 删除文件夹下所有文件 </summary>
    public static void DeleteFolder(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        try
        {
            // 1、首先判断文件或者文件路径是否存在
            if (File.Exists(path))
            {
                // 2、根据路径字符串判断是文件还是文件夹
                FileAttributes attr = File.GetAttributes(path);
                // 3、根据具体类型进行删除
                if (attr == FileAttributes.Directory)
                {
                    // 3.1、删除文件夹
                    Directory.Delete(path, true);
                }
            }
        }
        catch
        {
            Debug.LogError(string.Format("<color=#0000FF> 删除文件夹下所有文件-文件夹路径错误:{0}</color>", path));
        }
    }

    /// <summary> 文件是否存在 </summary>
    public static bool FileExist(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        return File.Exists(path);
    }

    /// <summary> 文件夹是否存在 </summary>
    public static bool FolderExist(string targetPath)
    {
        string path = targetPath.Replace("file://","");
        return Directory.Exists(path);
    }
}
