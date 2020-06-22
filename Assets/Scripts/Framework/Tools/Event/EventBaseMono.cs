using EventController;
using System;
using UnityEngine;
/// <summary>功能:事件基类,继承Mono</summary>
public class EventBaseMono : MonoBehaviour
{

    #region 添加事件
    /// <summary>添加事件</summary>
    protected void AddEventMsg(string str, Action cb, bool isRemove)
    {
        EventDispatcher.AddEventListener(str, () =>
        {
            cb();
        }, isRemove);
    }

    /// <summary>添加事件-1</summary>
    protected void AddEventMsg<T>(string str, Action<T> cb, bool isRemove)
    {
        EventDispatcher.AddEventListener(str, (T t) =>
        {
            cb(t);
        }, isRemove);
    }

    /// <summary>添加事件-2</summary>
    protected void AddEventMsg<T, T1>(string str, Action<T, T1> cb, bool isRemove)
    {
        EventDispatcher.AddEventListener(str, (T t, T1 t1) =>
        {
            cb(t, t1);
        }, isRemove);
    }

    /// <summary>添加事件-3</summary>
    protected void AddEventMsg<T, T1, T2>(string str, Action<T, T1, T2> cb, bool isRemove)
    {
        EventDispatcher.AddEventListener(str, (T t, T1 t1, T2 t2) =>
        {
            cb(t, t1, t2);
        }, isRemove);
    }
    #endregion

    #region 发送事件消息
    /// <summary>发送事件消息</summary>
    protected void SendEventMsg(string msg)
    {
        EventDispatcher.TriggerEvent(msg);
    }

    /// <summary>发送事件消息-1</summary>
    protected void SendEventMsg<T>(string msg, T t)
    {
        EventDispatcher.TriggerEvent(msg, t);
    }

    /// <summary>发送事件消息-2</summary>
    protected void SendEventMsg<T, T1>(string msg, T t, T1 t1)
    {
        EventDispatcher.TriggerEvent(msg, t, t1);
    }

    /// <summary>发送事件消息-3</summary>
    protected void SendEventMsg<T, T1, T2>(string msg, T t, T1 t1, T2 t2)
    {
        EventDispatcher.TriggerEvent(msg, t, t1, t2);
    }
    #endregion

    #region 移除事件
    /// <summary>移除所有事件</summary>
    protected void RemoveAllEvent()
    {
        EventDispatcher.RemoveAllEventListeners();
    }

    /// <summary>移除设置事件</summary>
    protected void RemoveDesignateEvent()
    {
        EventDispatcher.RemoveDesignateEvent();
    }

    /// <summary>移除指定事件</summary>
    protected void RemoveEvent(string msg)
    {
        EventDispatcher.RemoveEventListener(msg);
    }

    /// <summary>移除指定事件</summary>
    protected void RemoveEvent(string msg, Action cb)
    {
        EventDispatcher.RemoveEventListener(msg, cb);
    }

    /// <summary>移除指定事件-1</summary>
    protected void RemoveEvent<T>(string msg, Action<T> cb)
    {
        EventDispatcher.RemoveEventListener(msg, cb);
    }

    /// <summary>移除指定事件-2</summary>
    protected void RemoveEvent<T, T1>(string msg, Action<T, T1> cb)
    {
        EventDispatcher.RemoveEventListener(msg, cb);
    }

    /// <summary>移除指定事件-3</summary>
    protected void RemoveEvent<T, T1, T2>(string msg, Action<T, T1, T2> cb)
    {
        EventDispatcher.RemoveEventListener(msg, cb);
    }

    /// <summary>判断是否有该监听</summary>
    protected bool HasEvent(string msg, Action cb)
    {
        return EventDispatcher.HasEventListener(msg, cb);
    }
    #endregion
}
