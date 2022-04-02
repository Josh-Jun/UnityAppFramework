using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class RootScriptConfig
{
    [XmlElement]
    public bool IsDebug;//是否打印Log
    [XmlElement]
    public List<RootScript> RootScript;//脚本集合
}

[Serializable]
public class RootScript
{
    [XmlAttribute]
    public string ScriptName;//脚本名称(包含命名空间)
    [XmlAttribute]
    public string SceneName;//所属场景名称(用来初始化Root脚本Begin方法)
    [XmlText]
    public string LuaScriptPath;//Lua脚本路径
}