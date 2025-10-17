using System;
using UnityEngine;
using App.Core.Tools;
using App.Core.Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Master
{
    [Config]
    public class LanguageJsonConfig : Singleton<LanguageJsonConfig>, IConfig
    {
        private LanguageJsonData _data = new LanguageJsonData();
        private readonly Dictionary<int, Language> _dict = new Dictionary<int, Language>();
        private const string assetPath = "Assets/App/Scripts/Frame/Core/Master/Config/Json/LanguageJsonConfig.cs";
        public void Load()
        {
            var textAsset = AssetsMaster.Instance.LoadAssetSync<TextAsset>(assetPath);
            _data = JsonUtility.FromJson<LanguageJsonData>(textAsset.text);
            foreach (var data in _data.Languages)
            {
                _dict.Add(data.Id, data);
            }
        }
        public Language Get(int id)
        {
            _dict.TryGetValue(id, out var value);
            if (value == null)
            {
                throw new Exception($"找不到config数据,表名=[{nameof(LanguageJsonConfig)}],id=[{id}]");
            }
            return value;
        }
        public bool Contains(int id)
        {
            return _dict.ContainsKey(id);
        }
        public Dictionary<int, Language> GetAll()
        {
            return _dict;
        }
    }
    [System.Serializable]
    public class LanguageJsonData
    {
        public List<Language> Languages = new List<Language>();
    }
    [System.Serializable]
    public class Language
    {
        /// <summary>Id</summary>
        public int Id;
        /// <summary>Chinese</summary>
        public string Chinese;
        /// <summary>English</summary>
        public string English;
    }
}
