/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年10月21 13:24
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AppFrame.Attribute;
using AppFrame.Interface;
using AppFrame.Tools;
using UnityEngine;

namespace AppFrame.Manager
{
    public struct EventData
    {
        public object obj;
        public MethodInfo method;
    }
    public class EventManager : SingletonMono<EventManager>
    {
        private Dictionary<string, EventData> Events = new Dictionary<string, EventData>();
        protected override void OnSingletonMonoInit()
        {
            InitEventMethods();
        }

        private void InitEventMethods(string assemblyString = "App.Module")
        {
            var assembly = Assembly.Load(assemblyString);
            var types = assembly.GetTypes();
            
            
            foreach (var type in types)
            {
                var methods = type.GetMethods().Where(info => info.GetCustomAttributes(typeof(EventAttribute), false).Any()).ToList();
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttributes(typeof(EventAttribute), false).First() as EventAttribute;
                    // attribute!.Event(事件名称，用来触发事件)
                    // type.FullName(脚本名称，用来查找脚本实例)
                    // method(方法)
                    if (type.GetInterface(nameof(ISingleton)) != typeof(ISingleton) && !type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        Log.W($"{{{type.FullName}}} 未继承 ISingleton 使用Event特性标记的方法{{{method.Name}}}不起作用");
                        continue;
                    }
                    EventData data = new EventData()
                    {
                        method = method,
                    };
                    if (type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        var instances = FindObjectOfType(type);
                        data.obj = instances;
                    }
                    else
                    {
                        var singleton = SingletonManager.Instance.Get(type.FullName);
                        data.obj = singleton;
                    }
                    Events.TryAdd(attribute!.Event, data);
                }
            }
        }

        public void ExecuteEvent(string eventName, params object[] args)
        {
            if (Events.TryGetValue(eventName, out var data))
            {
                data.method.Invoke(data.obj, args);
            }
            else
            {
                Log.W($"{eventName} 事件未找到！");
            }
        }

        public bool HasEvent(string eventName)
        {
            return Events.ContainsKey(eventName);
        }

        public void RemoveEvent(string eventName)
        {
            Events.Remove(eventName);
        }

        public void RemoveAllEvent()
        {
            Events.Clear();
        }
    }
}