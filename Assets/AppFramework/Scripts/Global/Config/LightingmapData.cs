using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Lightingmap/LightingmapData")]
public class LightingmapData : ScriptableObject
{
    [FormerlySerializedAs("lightmapInfos")] public List<LightingmapInfo> lightingmapInfos = new List<LightingmapInfo>();
}
[System.Serializable]
public class LightingmapInfo
{
    public int lightmapIndex;
    public Vector4 lightmapScaleOffset;
}