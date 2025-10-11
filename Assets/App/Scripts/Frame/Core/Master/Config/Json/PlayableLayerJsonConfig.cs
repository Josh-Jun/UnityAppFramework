/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年10月11 14:47
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
    public class PlayableLayerJsonConfig : Singleton<PlayableLayerJsonConfig>, IConfig
    {
        private PlayableLayerJsonData _data = new PlayableLayerJsonData();
        private readonly Dictionary<int, PlayableLayer> _dict = new Dictionary<int, PlayableLayer>();
        public void Load()
        {
            var textAsset = AssetsMaster.Instance.LoadAssetSync<TextAsset>(AssetPath.PlayableLayerJsonData);
            _data = JsonUtility.FromJson<PlayableLayerJsonData>(textAsset.text);
            foreach (var data in _data.PlayableLayers)
            {
                _dict.Add(data.Id, data);
            }
        }
        public PlayableLayer Get(int id)
        {
            _dict.TryGetValue(id, out var value);
            if (value == null)
            {
                throw new Exception($"找不到config数据,表名=[{nameof(PlayableLayerJsonConfig)}],id=[{id}]");
            }
            return value;
        }
        public bool Contains(int id)
        {
            return _dict.ContainsKey(id);
        }
        public Dictionary<int, PlayableLayer> GetAll()
        {
            return _dict;
        }
    }
    [System.Serializable]
    public class PlayableLayerJsonData
    {
        public List<PlayableLayer> PlayableLayers = new List<PlayableLayer>();
    }
    [System.Serializable]
    public class PlayableLayer
    {
        /// <summary>Id</summary>
        public int Id;
        /// <summary>层级</summary>
        public int Layer;
        /// <summary>动画遮罩</summary>
        public string LayerAvatarMask;
    }
}
