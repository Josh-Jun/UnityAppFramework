using System;
using UnityEngine;

namespace EventController {
    public class EventDispatcher {
        private static EventController m_eventController = new EventController();

        //判断是否有该监听
        public static bool HasEventListener(string eventType,Delegate handler) {
            return m_eventController.HasEventListener(eventType,handler);
        }

        #region 注册消息
        //注册参数类型不同的消息调用;  
        public static void AddEventListener(string eventType,Action handler,bool isDesignateRemove = true) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }

        public static void AddEventListener<T>(string eventType,Action<T> handler,bool isDesignateRemove = true) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }
        public static void AddEventListener<T0,T1>(string eventType,Action<T0,T1> handler,bool isDesignateRemove = true) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }
        public static void AddEventListener<T0,T1,T2>(string eventType,Action<T0,T1,T2> handler,bool isDesignateRemove = true) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }
        public static void AddEventListener<T0,T1,T2,T3>(string eventType,Action<T0,T1,T2,T3> handler,bool isDesignateRemove = true) {
            m_eventController.AddEventListener(eventType,handler,isDesignateRemove);
        }
        #endregion

        #region 触发事件
        //触发某个事件
        public static void TriggerEvent(string eventType) {
            var isTrigger = m_eventController.DispatchEvent(eventType);
            if (!isTrigger) {
                Debug.Log($"{eventType}事件未注册");
            }
        }

        public static void TriggerEvent<T>(string eventType,T arg) {
            var isTrigger = m_eventController.DispatchEvent<T>(eventType, arg);
            if (!isTrigger) {
                Debug.Log($"{eventType}事件未注册");
            }
        }
        
        public static void TriggerEvent<T0,T1>(string eventType,T0 args0, T1 arg1) {
            var isTrigger = m_eventController.DispatchEvent<T0,T1>(eventType, args0, arg1);
            if (!isTrigger) {
                Debug.Log($"{eventType}事件未注册");
            }
        }
        public static void TriggerEvent<T0,T1,T2>(string eventType,T0 args0, T1 arg1, T2 arg2) {
            var isTrigger = m_eventController.DispatchEvent<T0,T1,T2>(eventType, args0, arg1, arg2);
            if (!isTrigger) {
                Debug.Log($"{eventType}事件未注册");
            }
        }
        public static void TriggerEvent<T0,T1,T2,T3>(string eventType,T0 args0, T1 arg1, T2 arg2, T3 arg3) {
            var isTrigger = m_eventController.DispatchEvent<T0,T1,T2,T3>(eventType, args0, arg1, arg2, arg3);
            if (!isTrigger) {
                Debug.Log($"{eventType}事件未注册");
            }
        }
        #endregion

        #region 移除事件
        //删除所有事件的所有监听
        public static void RemoveAllEventListeners() {
            m_eventController.RemoveAllEventListeners();
        }
        //移除指定的事件
        public static void RemoveDesignateEvent() {
            m_eventController.RemoveDesignateEvent();
        }
        //删除某个事件的一个监听
         public static void RemoveEventListener(string eventType) {
            m_eventController.RemoveEventListener(eventType);
        }

        public static bool RemoveEventListener(string eventType,Action handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }

        public static bool RemoveEventListener<T>(string eventType,Action<T> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }
        public static bool RemoveEventListener<T0,T1>(string eventType,Action<T0,T1> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }
        public static bool RemoveEventListener<T0,T1,T2>(string eventType,Action<T0,T1,T2> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }
        public static bool RemoveEventListener<T0,T1,T2,T3>(string eventType,Action<T0,T1,T2,T3> handler) {
            return m_eventController.RemoveEventListener(eventType,handler);
        }
        #endregion
    }
}

