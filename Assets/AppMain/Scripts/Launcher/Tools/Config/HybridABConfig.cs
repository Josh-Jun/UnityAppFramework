using System.Collections.Generic;

namespace Launcher
{
    [System.Serializable]
    public class HybridABConfig
    {
        public string GameVersion;//游戏版本号
        public string ResVersion; //资源版本号
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
        public string MD5; //MD5
        public string Size;//资源大小
        public string[] Dependencies;//依赖关系
    }
}