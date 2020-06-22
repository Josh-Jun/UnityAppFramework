using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindowBase : EventBaseMono
{
    private bool isInitWindow = true;//打开窗口初始化一次
    private void Awake()
    {
        InitEvent();
        RegisterEvent();
    }

    /// <summary>初始化</summary>
    protected virtual void InitEvent()
    {

    }

    /// <summary>注册消息事件,默认删除此事件</summary>
    protected virtual void RegisterEvent(bool isRemove = true)
    {
        //显隐
        AddEventMsg(name, (bool state) => {
            SetWindowActive(state);
        }, isRemove);
    }

    /// <summary>设置窗体显/隐</summary>
    public void SetWindowActive(bool isActive = true)
    {
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
            if (isInitWindow)
            {
                isInitWindow = false;
                InitWindow();
            }
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

    /// <summary>打开窗口</summary>
    protected virtual void OpenWindow()
    {
        
    }

    /// <summary>执行OpenWnd后,初始化窗口(只执行一次)</summary>
    protected virtual void InitWindow()
    {

    }

    /// <summary>关闭窗口</summary>
    protected virtual void CloseWindow()
    {

    }

}
