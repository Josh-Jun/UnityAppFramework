/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年10月21 13:24
 * function    :
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using App.Core.Helper;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    public struct EventData
    {
        public object obj;
        public MethodInfo method;
    }
    public class EventMaster : SingletonMono<EventMaster>
    {
        private readonly Dictionary<string, EventData> Events = new Dictionary<string, EventData>();

        private void Awake()
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
                    var data = new EventData()
                    {
                        method = method,
                    };
                    if (type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        var instances = FindObjectsOfType(type, true).First(obj => obj.GetType() == type);
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

        public void Execute(string eventName, params object[] args)
        {
            if (Events.TryGetValue(eventName, out var data))
            {
                var paramsInfo = data.method.GetParameters();
                if (paramsInfo.Length != args.Length)
                {
                    Log.W($"类对象{data.obj}中{eventName}事件对应的方法{data.method.Name}参数对应不上！！！");
                    return;
                }
                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i].GetType() == paramsInfo[i].ParameterType) continue;
                    Log.W($"参数类型不正确", ("目标类型", paramsInfo[i].ParameterType.Name), ("来源类型", args[i].GetType().Name));
                    return;
                }
                data.method.Invoke(data.obj, args);
            }
            else
            {
                Log.W($"{eventName} 事件未找到！");
            }
        }

        public bool Has(string eventName)
        {
            return Events.ContainsKey(eventName);
        }

        public void Remove(string eventName)
        {
            Events.Remove(eventName);
        }

        public void RemoveAll()
        {
            Events.Clear();
        }
    }
}