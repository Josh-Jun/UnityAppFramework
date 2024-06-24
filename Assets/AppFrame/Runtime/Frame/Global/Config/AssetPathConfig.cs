using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppFrame.Config
{
    [CreateAssetMenu(fileName = "AssetPathConfig", menuName = "App/AssetPathConfig")]
    [Serializable]
    public class AssetPathConfig : ScriptableObject
    {
        [Header("Asset Path Config")]
        [Tooltip("Asset Path 列表")]
        public List<AssetPath> AssetPath = new List<AssetPath>();
    }
    [Serializable]
    public struct AssetPath
    {
        public string name;
        public string path;
    }
}