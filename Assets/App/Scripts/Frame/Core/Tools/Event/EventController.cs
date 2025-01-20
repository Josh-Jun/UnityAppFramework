/*
 * 事件管理器 在哈希表中使用字符串，对委托和事件管理   
 */

using System;
using System.Collections;
using System.Collections.Generic;


namespace App.Core.Tools
{
    //事件订阅时分发次数
    public enum EventDispatcherMode
    {
        DEFAULT, //默认值，订阅后一直分发
        SINGLE_SHOT //只分发一次的订阅
    }

    public class EventController
    {
        //事件订阅列表, 两层Hashtable，第一层：key，事件类型字符串；第二层：key,EventListenerData转化成的字符串，value，结构体EventListenerData
        private Hashtable EventSubscription_Table = new Hashtable();

        private List<string> designateRemoveEventLst = new List<string>(); //指定移除的事件

        //接口，添加某个类型事件的监听
        public bool AddEventListener(string eventType, Action handler, bool isDesignateRemove,
            EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }

        public bool AddEventListener<T>(string eventType, Action<T> handler, bool isDesignateRemove,
            EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }

        public bool AddEventListener<T0, T1>(string eventType, Action<T0, T1> handler, bool isDesignateRemove,
            EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }

        public bool AddEventListener<T0, T1, T2>(string eventType, Action<T0, T1, T2> handler,
            bool isDesignateRemove, EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }

        public bool AddEventListener<T0, T1, T2, T3>(string eventType, Action<T0, T1, T2, T3> handler,
            bool isDesignateRemove, EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }
        
        public bool AddEventListener<T0, T1, T2, T3, T4>(string eventType, Action<T0, T1, T2, T3, T4> handler,
            bool isDesignateRemove, EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }
        
        public bool AddEventListener<T0, T1, T2, T3, T4, T5>(string eventType, Action<T0, T1, T2, T3, T4, T5> handler,
            bool isDesignateRemove, EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }
        
        public bool AddEventListener<T0, T1, T2, T3, T4, T5, T6>(string eventType, Action<T0, T1, T2, T3, T4, T5, T6> handler,
            bool isDesignateRemove, EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }
        
        public bool AddEventListener<T0, T1, T2, T3, T4, T5, T6, T7>(string eventType, Action<T0, T1, T2, T3, T4, T5, T6, T7> handler,
            bool isDesignateRemove, EventDispatcherMode eventDispatcherMode = EventDispatcherMode.DEFAULT)
        {
            return OnListenerAdding(eventType, handler, isDesignateRemove, eventDispatcherMode);
        }

        //内部实现
        private bool OnListenerAdding(string eventType, Delegate handler, bool isDesignateRemove,
            EventDispatcherMode eventDispatcherMode)
        {
            //返回值，是否插入成功
            bool isSuccess = false;
            //获取建立监听的对象
            object listenerObject = GetListenerObject(handler);

            if (listenerObject != null && eventType != null)
            {
                //判断是否有该类型事件
                if (!EventSubscription_Table.ContainsKey(eventType))
                {
                    EventSubscription_Table.Add(eventType, new Hashtable());
                    if (isDesignateRemove && !designateRemoveEventLst.Contains(eventType))
                    {
                        designateRemoveEventLst.Add(eventType);
                    }
                }

                //添加事件的监听
                Hashtable event_table = EventSubscription_Table[eventType] as Hashtable;
                //创建一个事件监听的相关数据对象
                EventListenerData eventListenerData =
                    new EventListenerData(listenerObject, eventType, handler, eventDispatcherMode);
                //转化为字符串  
                string eventListenerData_string = EventListenerData_To_String(eventListenerData);
                //判断该事件是否有这个类中这个函数的监听
                if (!event_table.Contains(eventListenerData_string))
                {
                    event_table.Add(eventListenerData_string, eventListenerData);
                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        //接口，判断是否有该监听
        public bool HasEventListener(string eventType, Delegate handler)
        {
            //返回结果
            bool isSuccess = false;
            //获取建立监听的对象
            object listenerObject = GetListenerObject(handler);
            //外层
            if (EventSubscription_Table.ContainsKey(eventType))
            {
                //内层
                Hashtable event_table = EventSubscription_Table[eventType] as Hashtable;
                EventListenerData eventListenerData = new EventListenerData(listenerObject, eventType, handler,
                    EventDispatcherMode.DEFAULT);
                string eventListenerData_string = EventListenerData_To_String(eventListenerData);
                //
                if (event_table.Contains(eventListenerData_string))
                {
                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        //删除某个事件的一个监听
        public void RemoveEventListener(string eventType)
        {
            EventSubscription_Table.Remove(eventType);
            designateRemoveEventLst.Remove(eventType);
        }

        public bool RemoveEventListener(string eventType, Delegate handler)
        {
            //返回结果
            bool isSuccess = false;
            if (HasEventListener(eventType, handler))
            {
                //外层
                Hashtable event_table = EventSubscription_Table[eventType] as Hashtable;
                //获取建立监听的对象
                object listenerObject = GetListenerObject(handler);
                //内层
                string eventListenerData_string = EventListenerData_To_String(
                    new EventListenerData(listenerObject, eventType, handler, EventDispatcherMode.DEFAULT));
                event_table.Remove(eventListenerData_string);
                isSuccess = true;
            }

            return isSuccess;
        }

        //删除所有事件的所有监听
        public void RemoveAllEventListeners()
        {
            EventSubscription_Table.Clear();
        }

        //移除指定的事件
        public void RemoveDesignateEvent()
        {
            for (int i = 0; i < designateRemoveEventLst.Count; i++)
            {
                if (EventSubscription_Table.ContainsKey(designateRemoveEventLst[i]))
                {
                    EventSubscription_Table.Remove(designateRemoveEventLst[i]);
                }
            }

            designateRemoveEventLst.Clear();
        }

        //触发某个事件
        public bool DispatchEvent(string eventType)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action action = temp as Action;
                        try
                        {
                            action();
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        //触发某个事件
        public bool DispatchEvent<T>(string eventType, T arg)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T> action = temp as Action<T>;
                        try
                        {
                            action(arg);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        //触发某个事件
        public bool DispatchEvent<T0, T1>(string eventType, T0 arg0, T1 arg1)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T0, T1> action = temp as Action<T0, T1>;
                        try
                        {
                            action(arg0, arg1);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        //触发某个事件
        public bool DispatchEvent<T0, T1, T2>(string eventType, T0 arg0, T1 arg1, T2 arg2)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T0, T1, T2> action = temp as Action<T0, T1, T2>;
                        try
                        {
                            action(arg0, arg1, arg2);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        //触发某个事件
        public bool DispatchEvent<T0, T1, T2, T3>(string eventType, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T0, T1, T2, T3> action = temp as Action<T0, T1, T2, T3>;
                        try
                        {
                            action(arg0, arg1, arg2, arg3);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        //触发某个事件
        public bool DispatchEvent<T0, T1, T2, T3, T4>(string eventType, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T0, T1, T2, T3, T4> action = temp as Action<T0, T1, T2, T3, T4>;
                        try
                        {
                            action(arg0, arg1, arg2, arg3, arg4);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }
        
        //触发某个事件
        public bool DispatchEvent<T0, T1, T2, T3, T4, T5>(string eventType, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T0, T1, T2, T3, T4, T5> action = temp as Action<T0, T1, T2, T3, T4, T5>;
                        try
                        {
                            action(arg0, arg1, arg2, arg3, arg4, arg5);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }
        
        //触发某个事件
        public bool DispatchEvent<T0, T1, T2, T3, T4, T5, T6>(string eventType, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T0, T1, T2, T3, T4, T5, T6> action = temp as Action<T0, T1, T2, T3, T4, T5, T6>;
                        try
                        {
                            action(arg0, arg1, arg2, arg3, arg4, arg5, arg6);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }
        
        //触发某个事件
        public bool DispatchEvent<T0, T1, T2, T3, T4, T5, T6, T7>(string eventType, T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            //返回值
            bool isSuccess = false;
            List<Delegate> handlerList = new List<Delegate>();
            if (OnDispatchEvent(eventType, ref handlerList))
            {
                if (handlerList != null)
                {
                    foreach (Delegate temp in handlerList)
                    {
                        Action<T0, T1, T2, T3, T4, T5, T6, T7> action = temp as Action<T0, T1, T2, T3, T4, T5, T6, T7>;
                        try
                        {
                            action(arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                        }
                        catch (Exception e)
                        {
                            Log.E(e);
                        }
                    }

                    isSuccess = true;
                }
            }

            return isSuccess;
        }
        
        //内部，触发某个事件
        private bool OnDispatchEvent(string eventType, ref List<Delegate> handlerList)
        {
            //返回值
            bool isSuccess = false;
            //外层
            if (EventSubscription_Table.ContainsKey(eventType))
            {
                //内层
                Hashtable event_table = EventSubscription_Table[eventType] as Hashtable;
                IEnumerator event_table_itor = event_table.GetEnumerator();
                DictionaryEntry dictionaryEntry; //外层某一对
                EventListenerData eventListenerData; //内层某一个元素
                ArrayList toBeRemoved_arraylist = new ArrayList(); //记录该事件需要单次调用的监听
                //循环该类型事件的所有监听
                while (event_table_itor.MoveNext())
                {
                    dictionaryEntry = (DictionaryEntry)event_table_itor.Current;
                    eventListenerData = dictionaryEntry.Value as EventListenerData;
                    handlerList.Add(eventListenerData.EventDelegate);
                    //单次
                    if (eventListenerData.EventListeningMode == EventDispatcherMode.SINGLE_SHOT)
                    {
                        toBeRemoved_arraylist.Add(eventListenerData);
                    }

                    isSuccess = true;
                }

                EventListenerData toBeRemoved_eventlistenerdata;
                for (int count_int = toBeRemoved_arraylist.Count - 1; count_int >= 0; count_int--)
                {
                    toBeRemoved_eventlistenerdata = toBeRemoved_arraylist[count_int] as EventListenerData;
                    RemoveEventListener(toBeRemoved_eventlistenerdata.EventName,
                        toBeRemoved_eventlistenerdata.EventDelegate);
                }
            }

            return isSuccess;
        }

        //一个事件监听的相关数据对象转化为字符串，作为第二层table的键值
        private string EventListenerData_To_String(EventListenerData aEventListenerData)
        {
            return aEventListenerData.EventListener.GetType().FullName + "_" +
                   aEventListenerData.EventListener.GetType().GUID + "_" + aEventListenerData.EventName + "_" +
                   (aEventListenerData.EventDelegate as System.Delegate).Method.Name.ToString();
        }

        //获取建立监听的对象
        private object GetListenerObject(Delegate aEventDelegate)
        {
            return aEventDelegate.Target;
        }
    }
}