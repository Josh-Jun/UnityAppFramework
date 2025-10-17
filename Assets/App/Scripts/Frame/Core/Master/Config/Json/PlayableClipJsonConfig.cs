using System;
using UnityEngine;
using App.Core.Tools;
using App.Core.Helper;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace App.Core.Master
{
    [Config]
    public class PlayableClipJsonConfig : Singleton<PlayableClipJsonConfig>, IConfig
    {
        private PlayableClipJsonData _data = new PlayableClipJsonData();
        private readonly Dictionary<int, PlayableClip> _dict = new Dictionary<int, PlayableClip>();
        private const string assetPath = "Assets/App/Scripts/Frame/Core/Master/Config/Json/PlayableClipJsonConfig.cs";
        public void Load()
        {
            var textAsset = AssetsMaster.Instance.LoadAssetSync<TextAsset>(assetPath);
            _data = JsonUtility.FromJson<PlayableClipJsonData>(textAsset.text);
            foreach (var data in _data.PlayableClips)
            {
                _dict.Add(data.Id, data);
            }
        }
        public PlayableClip Get(int id)
        {
            _dict.TryGetValue(id, out var value);
            if (value == null)
            {
                throw new Exception($"找不到config数据,表名=[{nameof(PlayableClipJsonConfig)}],id=[{id}]");
            }
            return value;
        }
        public bool Contains(int id)
        {
            return _dict.ContainsKey(id);
        }
        public Dictionary<int, PlayableClip> GetAll()
        {
            return _dict;
        }
    }
    [System.Serializable]
    public class PlayableClipJsonData
    {
        public List<PlayableClip> PlayableClips = new List<PlayableClip>();
    }
    [System.Serializable]
    public class PlayableClip
    {
        /// <summary>Id</summary>
        public int Id;
        /// <summary>所在层级</summary>
        public int Layer;
        /// <summary>动画路径</summary>
        public string Path;
    }
}
