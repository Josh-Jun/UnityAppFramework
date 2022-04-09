using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public enum AppTarget
{
    Android = 0,
    PicoVR = 1
}

[Serializable]
public class AppConfig
{
    [XmlElement]
    public BoolElement Debug;//是否打印Log
    [XmlElement]
    public BoolElement Server;//是否测试服生产服
    [XmlElement]
    public BoolElement Hotfix;//是否热更
    [XmlElement]
    public BoolElement LoadAB;//是否加载AB
    [XmlElement]
    public BoolElement XLua;//是否热更
}
[Serializable]
public class BoolElement
{
    [XmlText]
    public bool Value;
}
[Serializable]
public class StrElement
{
    [XmlText]
    public string Value;
}
[Serializable]
public class IntElement
{
    [XmlText]
    public int Value;
}
[Serializable]
public class FloatElement
{
    [XmlText]
    public float Value;
}