using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

namespace Data
{
    [System.Serializable]
    public class AssetBundleConfig
    {
        [XmlAttribute]
        public string GameVersion;//游戏版本号

        [XmlAttribute]
        public string Platform;//当前包的平台

        [XmlAttribute]
        public string Des;//热更描述

        [XmlElement]
        public List<Module> Modules;//AB包场景
    }

    [System.Serializable]
    public class Module
    {
        [XmlAttribute]
        public string ModuleName;//AB包场景名

        [XmlElement]
        public List<Folder> Folders; //文件夹
    }

    /// <summary>
    /// 单个补丁包
    /// </summary>
    [System.Serializable]
    public class Folder
    {
        [XmlAttribute]
        public string FolderName;//文件夹名

        [XmlAttribute]
        public string BundleName;//包名
        
        [XmlAttribute]
        public string Tag; //是否需要断点续传  0-不需要  1-需要

        [XmlAttribute]
        public string MD5; //MD5

        [XmlAttribute]
        public string Size;//资源大小
    }
}