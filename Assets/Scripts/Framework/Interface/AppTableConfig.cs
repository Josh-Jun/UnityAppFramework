using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public class AppTableConfig
{
    [XmlElement]
    public List<AppTable> AppTable;
}

[Serializable]
public class AppTable
{
    [XmlAttribute]
    public string TableName;
    [XmlText]
    public string TablePath;
}