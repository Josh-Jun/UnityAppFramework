using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace App.Core.Tools
{
    /// <summary> 功能：对象与XML  序列化/反序列化 工具 </summary>
    public class XmlTools
    {
        /// <summary> 将XML数据反序列化为指定类型对象 </summary>
        public static T ProtoDeSerialize<T>(byte[] msg) where T : class
        {
            try
            {
                using var ms = new MemoryStream();
                ms.Write(msg, 0, msg.Length);
                ms.Position = 0;
                var xs = new XmlSerializer(typeof(T));
                var obj = xs.Deserialize(ms);
                return (T)obj;
            }
            catch (Exception e)
            {
                Debug.LogError("序列化失败 ： " + e.ToString());
                return null;
            }
        }

        // 将类对象转换为 字符串xml 
        public static string ProtoStrSerialize<T>(object obj, bool isIndented = true)
        {
            var memoryStream = new MemoryStream();
            var xs = new XmlSerializer(typeof(T));
            using var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            if (isIndented)
            {
                xmlTextWriter.Formatting = Formatting.Indented;
            }

            var _namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[]
                { new XmlQualifiedName(string.Empty, string.Empty) });
            xs.Serialize(xmlTextWriter, obj, _namespaces);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            var encoding = new UTF8Encoding();
            return encoding.GetString(memoryStream.ToArray());
        }

        public static byte[] ProtoByteSerialize<T>(object obj, bool isIndented = true)
        {
            var memoryStream = new MemoryStream();
            var xs = new XmlSerializer(typeof(T));
            var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            if (isIndented)
            {
                xmlTextWriter.Formatting = Formatting.Indented;
            }

            var _namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[]
                { new XmlQualifiedName(string.Empty, string.Empty) });
            xs.Serialize(xmlTextWriter, obj, _namespaces);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            return memoryStream.ToArray();
        }

        /// <summary> Object是否可以转换xml </summary>
        public static bool Xmlserialize(string path, object obj)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            try
            {
                using var sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
                //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                //namespaces.Add(string.Empty, string.Empty);
                var xs = new XmlSerializer(obj.GetType());
                xs.Serialize(sw, obj);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("此类无法转换成xml " + obj.GetType() + "," + e);
            }

            return false;
        }

        /// <summary> 根据xml文件路径转换为T类型对象 </summary>
        public static T XmlDeserialize<T>(string path) where T : class
        {
            T t = null;
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                var xs = new XmlSerializer(typeof(T));
                t = (T)xs.Deserialize(fs);
            }
            catch (Exception e)
            {
                Debug.LogError("此xml无法转成二进制: " + path + "," + e);
            }

            return t;
        }

        /// <summary> Xml的反序列化转换为Object </summary>
        public static object XmlDeserialize(string path, Type type)
        {
            object obj = null;
            var xs = new XmlSerializer(type);
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                obj = xs.Deserialize(fs);
            }
            catch (Exception e)
            {
                Debug.LogError("此xml无法转成二进制: " + path + "," + e);
            }

            return obj;
        }

        public static XmlDocument Load(string xml_path)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xml_path);
            return xmlDocument;
        }

        public static void Save(XmlDocument xmlDocument, string xml_path)
        {
            xmlDocument.Save(xml_path);
        }
    }
}
