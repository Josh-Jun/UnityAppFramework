/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年10月21 13:24
 * function    :
 * ===============================================
 * */

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

        public void AddEventMethods(object obj)
        {
            var type = obj.GetType();
            var methods = type.GetMethods().Where(info => info.GetCustomAttributes(typeof(EventAttribute), false).Any())
                .ToList();
            foreach (var ea in from method in methods
                     let ea = method.GetCustomAttributes(typeof(EventAttribute), false).First() as EventAttribute
                     let data = new EventData
                     {
                         method = method,
                         obj = obj
                     }
                     where !Events.TryAdd(ea!.Event, data)
                     select ea)
            {
                Log.W($"{ea!.Event} 该事件已存在");
            }
        }

        public void AddEventMethods(ILogic logic)
        {
            var type = logic.GetType();
            var methods = type.GetMethods().Where(info => info.GetCustomAttributes(typeof(EventAttribute), false).Any())
                .ToList();
            foreach (var ea in from method in methods
                     let ea = method.GetCustomAttributes(typeof(EventAttribute), false).First() as EventAttribute
                     let data = new EventData
                     {
                         method = method,
                         obj = logic
                     }
                     where !Events.TryAdd(ea!.Event, data)
                     select ea)
            {
                Log.W($"{ea!.Event} 该事件已存在");
            }
        }
        /// <summary>
        /// 在所有Logic和View加载完成后初始化所有事件
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="assemblyString"></param>
        public void InitEventMethods(Dictionary<string, List<ILogic>> pairs, string assemblyString = "App.Module")
        {
            // 初始化所有Logic脚本的Event特性
            var logics = pairs.SelectMany(x => x.Value).ToList();
            foreach (var logic in logics)
            {
                AddEventMethods(logic);
            }
            // 添加所有View脚本的Event特性
            var views = ViewMaster.Instance.GetAllView();
            foreach (var view in views)
            {
                AddEventMethods(view);
            }
            // 添加所有MonoBehaviour脚本的Event特性
            // 脚本必须提前挂到GameObject上,并且GameObject要在场景中已经加载,在所有View加载完成之后,可能需要手动添加
            var assembly = Assembly.Load(assemblyString);
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var va = type.GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
                if (va is ViewOfAttribute) continue;
                var la = type.GetCustomAttributes(typeof(LogicOfAttribute), false).FirstOrDefault();
                if (la is LogicOfAttribute) continue;
                var methods = type.GetMethods()
                    .Where(info => info.GetCustomAttributes(typeof(EventAttribute), false).Any()).ToList();
                foreach (var method in methods)
                {
                    var ea = method.GetCustomAttributes(typeof(EventAttribute), false).FirstOrDefault() as EventAttribute;
                    // attribute!.Event(事件名称，用来触发事件)
                    // type.FullName(脚本名称，用来查找脚本实例)
                    // method(方法)
                    if (!type.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        Log.W($"{{{type.FullName}}} 未继承 MonoBehaviour 使用Event特性标记的方法{{{method.Name}}}不起作用");
                        continue;
                    }

                    var instances = FindObjectsOfType(type, false).FirstOrDefault(obj => obj.GetType() == type);
                    if (!instances)
                    {
                        Log.W($"{{{type.FullName}}} 未找到实例对象");
                        continue;
                    }

                    var data = new EventData
                    {
                        method = method,
                        obj = instances
                    };
                    if (!Events.TryAdd(ea!.Event, data))
                    {
                        Log.W($"{ea!.Event} 该事件已存在");
                    }
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