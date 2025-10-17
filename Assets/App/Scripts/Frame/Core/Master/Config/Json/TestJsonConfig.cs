using System;
using UnityEngine;
using App.Core.Tools;
using App.Core.Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Master
{
    [Config]
    public class TestJsonConfig : Singleton<TestJsonConfig>, IConfig
    {
        private TestJsonData _data = new TestJsonData();
        private readonly Dictionary<int, Test> _dict = new Dictionary<int, Test>();
        private const string assetPath = "Assets/App/Scripts/Frame/Core/Master/Config/Json/TestJsonConfig.cs";
        public void Load()
        {
            var textAsset = AssetsMaster.Instance.LoadAssetSync<TextAsset>(assetPath);
            _data = JsonUtility.FromJson<TestJsonData>(textAsset.text);
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
                throw new Exception($"找不到config数据,表名=[{nameof(TestJsonConfig)}],id=[{id}]");
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
    public class TestJsonData
    {
        public List<Test> Tests = new List<Test>();
    }
    [System.Serializable]
    public class Test
    {
        /// <summary>Id</summary>
        public int Id;
        /// <summary>字符串</summary>
        public string Str;
        /// <summary>字符串数组</summary>
        public string[] Strs;
        /// <summary>整数</summary>
        public int Number;
        /// <summary>整数数组</summary>
        public int[] Numbers;
        /// <summary>浮点数</summary>
        public float SmallNumber;
        /// <summary>浮点数组</summary>
        public float[] SmallNumbers;
        /// <summary>Vector3</summary>
        public Vector3 Pos;
        /// <summary>Vector3数组</summary>
        public Vector3[] PosArr;
    }
}
