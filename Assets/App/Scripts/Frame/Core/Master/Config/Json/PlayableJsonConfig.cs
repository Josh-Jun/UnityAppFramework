using System;
using UnityEngine;
using App.Core.Tools;
using App.Core.Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Master
{
    [Config]
    public class PlayableJsonConfig : Singleton<PlayableJsonConfig>, IConfig
    {
        private PlayableJsonData _data = new PlayableJsonData();
        private readonly Dictionary<int, Playable> _dict = new Dictionary<int, Playable>();
        private const string location = "Assets/Bundles/Builtin/Configs/Json/PlayableJsonData.json";
        public void Load()
        {
            var textAsset = AssetsMaster.Instance.LoadAssetSync<TextAsset>(location);
            _data = JsonUtility.FromJson<PlayableJsonData>(textAsset.text);
            foreach (var data in _data.Playables)
            {
                _dict.Add(data.Id, data);
            }
        }
        public Playable Get(int id)
        {
            _dict.TryGetValue(id, out var value);
            if (value == null)
            {
                throw new Exception($"找不到config数据,表名=[{nameof(PlayableJsonConfig)}],id=[{id}]");
            }
            return value;
        }
        public bool Contains(int id)
        {
            return _dict.ContainsKey(id);
        }
        public Dictionary<int, Playable> GetAll()
        {
            return _dict;
        }
    }
    [System.Serializable]
    public class PlayableJsonData
    {
        public List<Playable> Playables = new List<Playable>();
    }
    [System.Serializable]
    public class Playable
    {
        /// <summary>Id</summary>
        public int Id;
        /// <summary>动画名称</summary>
        public string Name;
        /// <summary>动画片段</summary>
        public int[] Clips;
        /// <summary>是否循环</summary>
        public bool IsLoop;
        /// <summary>动画图标</summary>
        public string Icon;
        /// <summary>动画道具</summary>
        public string Prop;
        /// <summary>道具节点</summary>
        public string PropRoot;
        /// <summary>道具偏移位置</summary>
        public Vector3 PropOffset;
        /// <summary>道具角度</summary>
        public Vector3 PropAngle;
        /// <summary>是否可打断</summary>
        public bool CanStop;
        /// <summary>动作类型</summary>
        public int Type;
    }
}
