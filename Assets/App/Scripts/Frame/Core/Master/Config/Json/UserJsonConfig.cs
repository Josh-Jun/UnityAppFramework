using System;
using UnityEngine;
using App.Core.Tools;
using App.Core.Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Master
{
    [Config]
    public class UserJsonConfig : Singleton<UserJsonConfig>, IConfig
    {
        private UserJsonData _data = new UserJsonData();
        private readonly Dictionary<int, User> _dict = new Dictionary<int, User>();
        public void Load()
        {
            var path = $"Assets/Bundles/Builtin/Configs/Json/UserJsonData.json";
            var textAsset = AssetsMaster.Instance.LoadAsset<TextAsset>(path);
            _data = JsonUtility.FromJson<UserJsonData>(textAsset.text);
            foreach (var data in _data.Users)
            {
                _dict.Add(data.Id, data);
            }
        }
        public User Get(int id)
        {
            _dict.TryGetValue(id, out var value);
            if (value == null)
            {
                throw new Exception($"找不到config数据,表名=[{nameof(UserJsonConfig)}],id=[{id}]");
            }
            return value;
        }
        public bool Contains(int id)
        {
            return _dict.ContainsKey(id);
        }
        public Dictionary<int, User> GetAll()
        {
            return _dict;
        }
    }
    [System.Serializable]
    public class UserJsonData
    {
        public List<User> Users = new List<User>();
    }
    [System.Serializable]
    public class User
    {
        public int Id;
        public int UserId;
        public long PhoneNumber;
        public string NickName;
        public int Sex;
    }
}
