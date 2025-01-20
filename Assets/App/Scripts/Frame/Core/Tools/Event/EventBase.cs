using System;

namespace App.Core.Tools
{
    /// <summary>功能:事件基类</summary>
    public class EventBase
    {
        #region 添加事件

        /// <summary>添加事件-不带参数</summary>
        public void AddEventMsg(string str, Action cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T>(string str, Action<T> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T0, T1>(string str, Action<T0, T1> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T0, T1, T2>(string str, Action<T0, T1, T2> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T0, T1, T2, T3>(string str, Action<T0, T1, T2, T3> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T0, T1, T2, T3, T4>(string str, Action<T0, T1, T2, T3, T4> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T0, T1, T2, T3, T4, T5>(string str, Action<T0, T1, T2, T3, T4, T5> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T0, T1, T2, T3, T4, T5, T6>(string str, Action<T0, T1, T2, T3, T4, T5, T6> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }

        /// <summary>添加事件-带参数</summary>
        public void AddEventMsg<T0, T1, T2, T3, T4, T5, T6, T7>(string str, Action<T0, T1, T2, T3, T4, T5, T6, T7> cb, bool isRemove = true)
        {
            EventDispatcher.AddEventListener(str, cb, isRemove);
        }
        #endregion

        #region 发送事件消息

        /// <summary>发送事件消息-不带参数</summary>
        public void SendEventMsg(string msg)
        {
            EventDispatcher.TriggerEvent(msg);
        }
        
        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T>(string msg, T args)
        {
            EventDispatcher.TriggerEvent(msg, args);
        }

        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T0, T1>(string msg, T0 arg0, T1 arg1)
        {
            EventDispatcher.TriggerEvent(msg, arg0, arg1);
        }

        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T0, T1, T2>(string msg, T0 arg0, T1 arg1, T2 arg2)
        {
            EventDispatcher.TriggerEvent(msg, arg0, arg1, arg2);
        }

        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T0, T1, T2, T3>(string msg, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            EventDispatcher.TriggerEvent(msg, arg0, arg1, arg2, arg3);
        }

        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T0, T1, T2, T3, T4>(string msg, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            EventDispatcher.TriggerEvent(msg, arg0, arg1, arg2, arg3, arg4);
        }

        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T0, T1, T2, T3, T4, T5>(string msg, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            EventDispatcher.TriggerEvent(msg, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T0, T1, T2, T3, T4, T5, T6>(string msg, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            EventDispatcher.TriggerEvent(msg, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>发送事件消息-带参数</summary>
        public void SendEventMsg<T0, T1, T2, T3, T4, T5, T6, T7>(string msg, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            EventDispatcher.TriggerEvent(msg, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
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

        /// <summary>移除事件</summary>
        public void RemoveEvent(string msg)
        {
            EventDispatcher.RemoveEventListener(msg);
        }

        /// <summary>移除指定事件-不带参数</summary>
        public void RemoveEvent(string msg, Action cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T>(string msg, Action<T> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T0, T1>(string msg, Action<T0, T1> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T0, T1, T2>(string msg, Action<T0, T1, T2> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T0, T1, T2, T3>(string msg, Action<T0, T1, T2, T3> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T0, T1, T2, T3, T4>(string msg, Action<T0, T1, T2, T3, T4> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T0, T1, T2, T3, T4, T5>(string msg, Action<T0, T1, T2, T3, T4, T5> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T0, T1, T2, T3, T4, T5, T6>(string msg, Action<T0, T1, T2, T3, T4, T5, T6> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>移除指定事件-带参数</summary>
        public void RemoveEvent<T0, T1, T2, T3, T4, T5, T6, T7>(string msg, Action<T0, T1, T2, T3, T4, T5, T6, T7> cb)
        {
            EventDispatcher.RemoveEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T>(string msg, Action<T> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T0, T1>(string msg, Action<T0, T1> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T0, T1, T2>(string msg, Action<T0, T1, T2> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T0, T1, T2, T3>(string msg, Action<T0, T1, T2, T3> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T0, T1, T2, T3, T4>(string msg, Action<T0, T1, T2, T3, T4> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T0, T1, T2, T3, T4, T5>(string msg, Action<T0, T1, T2, T3, T4, T5> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T0, T1, T2, T3, T4, T5, T6>(string msg, Action<T0, T1, T2, T3, T4, T5, T6> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-带参数</summary>
        public bool HasEvent<T0, T1, T2, T3, T4, T5, T6, T7>(string msg, Action<T0, T1, T2, T3, T4, T5, T6, T7> cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        /// <summary>判断是否有该监听-不带参数</summary>
        public bool HasEvent(string msg, Action cb)
        {
            return EventDispatcher.HasEventListener(msg, cb);
        }

        #endregion
    }
}