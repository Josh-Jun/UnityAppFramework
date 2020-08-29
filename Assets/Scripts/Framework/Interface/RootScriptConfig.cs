using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class RootScriptConfig
{
    [XmlElement]
    public List<RootScript> RootScript;//脚本集合
}

[Serializable]
public class RootScript
{
    [XmlAttribute]
    public string ScriptName;//脚本名称(包含明明空间)
    [XmlText]
    public string LuaScriptPath;//Lua脚本路径
}