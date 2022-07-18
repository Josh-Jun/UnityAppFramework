using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowBase : EventBaseMono
{
    [Obsolete("此方法已弃用，请使用InitWindow方法", true)]
    protected virtual void Awake()
    {
        InitWindow();
        RegisterEvent();
    }
    [Obsolete("此方法已弃用，请使用RegisterEvent方法", true)]
    protected virtual void Start()
    {

    }
    /// <summary>初始化UI窗口</summary>
    protected virtual void InitWindow()
    {

    }

    /// <summary>注册消息事件,默认删除此事件</summary>
    protected virtual void RegisterEvent()
    {
        //显隐
        AddEventMsg<bool>(name, SetWindowActive);
    }

    /// <summary>打开窗口</summary>
    protected virtual void OpenWindow()
    {

    }

    /// <summary>关闭窗口</summary>
    protected virtual void CloseWindow()
    {

    }

    /// <summary>设置窗体显/隐</summary>
    public void SetWindowActive(bool isActive = true)
    {
        if (this == null) return;
        if (gameObject != null)
        {
            if (gameObject.activeSelf != isActive)
            {
                gameObject.SetActive(isActive);
            }
        }
        if (isActive)
        {
            OpenWindow();
        }
        else
        {
            CloseWindow();
        }
    }

    /// <summary>获取窗体的状态</summary>
    public bool GetWindowActive()
    {
        return gameObject.activeSelf;
    }
    public void SetAsLastSibling()
    {
        transform.SetAsLastSibling();
    }
    public void SetAsFirstSibling()
    {
        transform.SetAsFirstSibling();
    }
    public void SetSiblingIndex(int index)
    {
        transform.SetSiblingIndex(index);
    }
    public void SetLightingmapData(LightingmapData lightingmap)
    {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

        if (renderers != null && renderers.Length > 0)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].lightmapIndex = lightingmap.lightingmapInfos[i].lightmapIndex;
                    renderers[i].lightmapScaleOffset = lightingmap.lightingmapInfos[i].lightmapScaleOffset;
                }
            }
        }
    }
}
