using System;
using UnityEngine;
using App.Core.Tools;
using App.Core.Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Master
{
    [Config]
    public class TestXmlConfig : Singleton<TestXmlConfig>, IConfig
    {
        private TestXmlData _data = new TestXmlData();
        private readonly Dictionary<int, Test> _dict = new Dictionary<int, Test>();
        public void Load()
        {
            var path = $"Assets/Bundles/Builtin/Configs/Xml/TestXmlData.xml";
            var textAsset = AssetsMaster.Instance.LoadAsset<TextAsset>(path);
            _data = XmlTools.ProtoDeSerialize<TestXmlData>(textAsset.bytes);
            foreach (var data in _data.Tests)
            {
                _dict.Add(data.Id, data);
            }
        }
        public Test Get(int id)
        {
            _dict.TryGetValue(id, out var value);
            if (value == null)
            {
                throw new Exception($"找不到config数据,表名=[{nameof(TestXmlConfig)}],id=[{id}]");
            }
            return value;
        }
        public bool Contains(int id)
        {
            return _dict.ContainsKey(id);
        }
        public Dictionary<int, Test> GetAll()
        {
            return _dict;
        }
    }
    [System.Serializable]
    public class TestXmlData
    {
        [XmlElement("Tests")]
        public List<Test> Tests = new List<Test>();
    }
    [System.Serializable]
    public class Test
    {
        [XmlAttribute("Id")]
        public int Id;
        [XmlAttribute("UserId")]
        public int UserId;
        [XmlAttribute("PhoneNumber")]
        public long PhoneNumber;
        [XmlAttribute("Sex")]
        public int Sex;
        [XmlAttribute("Age")]
        public int Age;
    }
}
