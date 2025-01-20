/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月20 18:36
 * function    : 
 * ===============================================
 * */
using System;
using UnityEngine;
using App.Core.Tools;
using App.Core.Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Master
{
    [Config]
    public class AdminXmlConfig : Singleton<AdminXmlConfig>, IConfig
    {
        private AdminXmlData _data = new AdminXmlData();
        private readonly Dictionary<int, Admin> _dict = new Dictionary<int, Admin>();
        public void Load()
        {
            var path = $"Assets/Bundles/Builtin/Configs/Xml/AdminXmlData.xml";
            var textAsset = AssetsMaster.Instance.LoadAsset<TextAsset>(path);
            _data = XmlTools.ProtoDeSerialize<AdminXmlData>(textAsset.bytes);
            foreach (var data in _data.Admins)
            {
                _dict.Add(data.Id, data);
            }
        }
        public Admin Get(int id)
        {
            _dict.TryGetValue(id, out var value);
            if (value == null)
            {
                throw new Exception($"找不到config数据,表名=[{nameof(AdminXmlConfig)}],id=[{id}]");
            }
            return value;
        }
        public bool Contains(int id)
        {
            return _dict.ContainsKey(id);
        }
        public Dictionary<int, Admin> GetAll()
        {
            return _dict;
        }
    }
    [System.Serializable]
    public class AdminXmlData
    {
        [XmlElement("Admins")]
        public List<Admin> Admins = new List<Admin>();
    }
    [System.Serializable]
    public class Admin
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
