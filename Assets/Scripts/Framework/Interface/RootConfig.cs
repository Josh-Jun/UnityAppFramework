using System.Collections.Generic;
using System.Xml.Serialization;

[System.Serializable]
public class RootConfig
{
    [XmlElement]
    public List<string> ScriptNames;//脚本名称(包括包名)
}
