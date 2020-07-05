using EventController;
using System;
/// <summary>功能:事件基类</summary>
public class EventBase
{
    #region 添加事件
    /// <summary>添加事件</summary>
    public void AddEventMsg(string str, Action cb, bool isRemove)
    {
        EventDispatcher.AddEventListener(str, () => {
            cb?.Invoke();
        }, isRemove);
    }

    /// <summary>添加事件-带参数</summary>
    public void AddEventMsgPro(string str, Action<object[]> cb, bool isRemove)
    {
        EventDispatcher.AddEventListener(str, (object[] obj) => {
            cb?.Invoke(obj);
        }, isRemove);
    }
    #endregion

    #region 发送事件消息

    /// <summary>发送事件消息-带参数</summary>
    public void SendEventMsg(string msg, params object[] obj)
    {
        EventDispatcher.TriggerEvent(msg, obj);
    }

    #endregion

    #region 移除事件
    /// <summary>移除所有事件</summary>
    public void RemoveAllEvent()
    {
        EventDispatcher.RemoveAllEventListeners();
    }

    /// <summary>移除设置事件</summary>
    public void RemoveDesignateEvent()
    {
        EventDispatcher.RemoveDesignateEvent();
    }

    public void RemoveEvent(string msg)
    {
        EventDispatcher.RemoveEventListener(msg);
    }

    /// <summary>移除指定事件</summary>
    public void RemoveEvent(string msg, Action cb)
    {
        EventDispatcher.RemoveEventListener(msg, cb);
    }

    /// <summary>移除指定事件-带参数</summary>
    public void RemoveEventPro(string msg, Action<object[]> cb)
    {
        EventDispatcher.RemoveEventListener(msg, cb);
    }


    /// <summary>判断是否有该监听</summary>
    public bool HasEvent(string msg, Action cb)
    {
        return EventDispatcher.HasEventListener(msg, cb);
    }
    #endregion
}
