/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2025年1月23 10:40
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace App.Core.Tools
{
    public class AnimationEventListener : MonoBehaviour
    {
        private readonly Dictionary<string, (bool, Action<string>)> StringCallbacks = new Dictionary<string, (bool, Action<string>)>();
        private readonly Dictionary<string, (bool, Action<int>)> IntCallbacks = new Dictionary<string, (bool, Action<int>)>();
        private readonly Dictionary<string, (bool, Action<float>)> FloatCallbacks = new Dictionary<string, (bool, Action<float>)>();
        private readonly Dictionary<string, (bool, Action<Object>)> ObjectCallbacks = new Dictionary<string, (bool, Action<Object>)>();
        
        public void AddStringListener(string eventName, Action<string> callback, bool AutoClear = false)
        {
            if (StringCallbacks.ContainsKey(eventName))
            {
                StringCallbacks[eventName] = (AutoClear, callback);
            }
            else
            {
                StringCallbacks.Add(eventName, (AutoClear, callback));
            }
        }
        
        public void AddIntListener(string eventName, Action<int> callback, bool AutoClear = false)
        {
            if (IntCallbacks.ContainsKey(eventName))
            {
                IntCallbacks[eventName] = (AutoClear, callback);
            }
            else
            {
                IntCallbacks.Add(eventName, (AutoClear, callback));
            }
        }

        public void AddFloatListener(string eventName, Action<float> callback, bool AutoClear = false)
        {
            if (FloatCallbacks.ContainsKey(eventName))
            {
                FloatCallbacks[eventName] = (AutoClear, callback);
            }
            else
            {
                FloatCallbacks.Add(eventName, (AutoClear, callback));
            }
        }

        public void AddObjectListener(string eventName, Action<Object> callback, bool AutoClear = false)
        {
            if (ObjectCallbacks.ContainsKey(eventName))
            {
                ObjectCallbacks[eventName] = (AutoClear, callback);
            }
            else
            {
                ObjectCallbacks.Add(eventName, (AutoClear, callback));
            }
        }

        public void RemoveStringListener(string eventName)
        {
            if (StringCallbacks.ContainsKey(eventName))
            {
                StringCallbacks.Remove(eventName);
            }
        }

        public void RemoveIntListener(string eventName)
        {
            if (IntCallbacks.ContainsKey(eventName))
            {
                IntCallbacks.Remove(eventName);
            }
        }

        public void RemoveFloatListener(string eventName)
        {
            if (FloatCallbacks.ContainsKey(eventName))
            {
                FloatCallbacks.Remove(eventName);
            }
        }

        public void RemoveObjectListener(string eventName)
        {
            if (ObjectCallbacks.ContainsKey(eventName))
            {
                ObjectCallbacks.Remove(eventName);
            }
        }
        
        public void AnimationEventStringCallback(string parameter)
        {
            foreach (var valueTuple in StringCallbacks)
            {
                valueTuple.Value.Item2?.Invoke(parameter);
                if (valueTuple.Value.Item1)
                {
                    StringCallbacks.Remove(valueTuple.Key);
                }
            }
        }
        
        public void AnimationEventIntCallback(int parameter)
        {
            foreach (var valueTuple in IntCallbacks)
            {
                valueTuple.Value.Item2?.Invoke(parameter);
                if (valueTuple.Value.Item1)
                {
                    IntCallbacks.Remove(valueTuple.Key);
                }
            }
        }
        
        public void AnimationEventFloatCallback(float parameter)
        {
            foreach (var valueTuple in FloatCallbacks)
            {
                valueTuple.Value.Item2?.Invoke(parameter);
                if (valueTuple.Value.Item1)
                {
                    FloatCallbacks.Remove(valueTuple.Key);
                }
            }
        }

        public void AnimationEventObjectCallback(Object parameter)
        {
            foreach (var valueTuple in ObjectCallbacks)
            {
                valueTuple.Value.Item2?.Invoke(parameter);
                if (valueTuple.Value.Item1)
                {
                    ObjectCallbacks.Remove(valueTuple.Key);
                }
            }
        }
    }
}
