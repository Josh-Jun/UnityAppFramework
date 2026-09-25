/* *
 * ===============================================
 * author      : Junzi@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2026年9月5 16:20
 * function    : 
 * ===============================================
 * */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Core.Master
{
    public partial class XMaster
    {
        private static readonly Dictionary<string,CrossAssemblyEventConfig> _scriptableObjects = new();
        private static readonly Dictionary<string,CrossAssemblyEventArgsConfig> _scriptableObjectsArg = new();

        public void Execute(string path, CrossAssemblyEventArgsDataBase data)
        {
            if (!_scriptableObjectsArg.TryGetValue(path, out var config))
            {
                config = AssetsMaster.Instance.LoadAssetSync<CrossAssemblyEventArgsConfig>(path);
                if (config)
                {
                    _scriptableObjectsArg.Add(path, config);
                }
                else
                {
                    Debug.Log("No ScriptableObject");
                    return;
                }
            }
            config.Execute(data);
        }
        public void Execute(string path)
        {
            if (!_scriptableObjects.TryGetValue(path, out var config))
            {
                config = AssetsMaster.Instance.LoadAssetSync<CrossAssemblyEventConfig>(path);
                if (config)
                {
                    _scriptableObjects.Add(path, config);
                }
                else
                {
                    Debug.Log("No ScriptableObject");
                    return;
                }
            }
            config.Execute();
        }
    }
}
