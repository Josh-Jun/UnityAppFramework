using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class AppScriptConfig
{
    [XmlAttribute]
    public string MainSceneName;//主场景名(首次进入的场景)
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