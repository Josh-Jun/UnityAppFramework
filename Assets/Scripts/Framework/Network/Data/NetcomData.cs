using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class NetcomData
{
    public float progress;
    public bool isDown;
    public bool isError;
    public string error;
    public string text;
    public byte[] data;
    public Texture2D texture;
    public AudioClip audioclip;
    public AssetBundle assetbundle;
}
