/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年7月28 16:46
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HybridCLR;
using UnityEngine;
using YooAsset;

namespace App.Runtime.Code
{
    public class CodeLoader
    {
        public static void Start()
        {
            if (Global.AppConfig.AssetPlayMode == EPlayMode.EditorSimulateMode) return;
            Debug.Log("加载热更dll");
            LoadMetadataForAOTAssemblies();
            LoadHotfixAssemblies();
        }
        

        private static void LoadMetadataForAOTAssemblies()
        {
            foreach (var aotName in Global.AOTMetaAssemblyNames)
            {
#if !UNITY_EDITOR
                var textAsset = Assets.LoadAssetSync($"{Global.DllBasePath}/{aotName}.bytes").AssetObject as TextAsset;
                if (textAsset) RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
#endif
            }
        }

        private static void LoadHotfixAssemblies()
        {
            foreach (var assemblyName in Global.HotfixAssemblyNames)
            {
#if !UNITY_EDITOR
                var textAsset = Assets.LoadAssetSync($"{Global.DllBasePath}/{assemblyName}.dll.bytes").AssetObject as TextAsset;
                if (!textAsset) continue;
                var assembly = System.Reflection.Assembly.Load(textAsset.bytes);
                Global.AssemblyPairs.Add(assemblyName, assembly);
#else
                 var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == assemblyName);
                 Global.AssemblyPairs.Add(assemblyName, assembly);
#endif
            }
        }
    }
}