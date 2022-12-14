using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppFrame.Config
{
    [System.Serializable]
    public class AssetBundleConfig
    {
        public string GameVersion;//游戏版本号
        public string Platform;//当前包的平台
        public string Des;//热更描述
        public List<Module> Modules;//AB包场景
    }

    [System.Serializable]
    public class Module
    {
        public string ModuleName;//AB包场景名
        public List<Folder> Folders; //文件夹
    }

    /// <summary>
    /// 单个补丁包
    /// </summary>
    [System.Serializable]
    public class Folder
    {
        public string FolderName;//文件夹名
        public string BundleName;//包名
        public string Tag; //是否需要断点续传  0-不需要  1-需要
        public string Mold; //是否本地包
        public string MD5; //MD5
        public string Size;//资源大小
    }
}