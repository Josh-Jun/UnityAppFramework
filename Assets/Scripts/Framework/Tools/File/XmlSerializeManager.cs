using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
/// <summary> 功能：对象与XML  序列化/反序列化 工具 </summary>
public class XmlSerializeManager
{
    /// <summary> 将XML数据反序列化为指定类型对象 </summary>
    public static T ProtoDeSerialize<T>(byte[] msg) where T : class
    {
        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(msg, 0, msg.Length);
                ms.Position = 0;
                XmlSerializer xs = new XmlSerializer(typeof(T));
                object obj = xs.Deserialize(ms);
                return (T)obj;
            }
        }
        catch (Exception e)
        {
            Debuger.LogError("序列化失败 ： " + e.ToString());
            return null;
        }
    }

    /// <summary> Object是否可以转换xml </summary>
    public static bool Xmlserialize(string path, object obj)
    {
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    //namespaces.Add(string.Empty, string.Empty);
                    XmlSerializer xs = new XmlSerializer(obj.GetType());
                    xs.Serialize(sw, obj);
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Debuger.LogError("此类无法转换成xml " + obj.GetType() + "," + e);
        }
        return false;
    }

    /// <summary> 根据xml文件路径转换为T类型对象 </summary>
    public static T XmlDeserialize<T>(string path) where T : class
    {
        T t = default;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                t = (T)xs.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debuger.LogError("此xml无法转成二进制: " + path + "," + e);
        }
        return t;
    }

    /// <summary> Xml的反序列化转换为Object </summary>
    public static object XmlDeserialize(string path, Type type)
    {
        object obj = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xs = new XmlSerializer(type);
                obj = xs.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debuger.LogError("此xml无法转成二进制: " + path + "," + e);
        }
        return obj;
    }
}
