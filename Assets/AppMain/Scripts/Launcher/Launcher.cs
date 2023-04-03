using System;
using System.Collections;
using System.Collections.Generic;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;

namespace Launcher
{
    public class Launcher : MonoBehaviour
    {
        public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
            "UnityEngine.CoreModule.dll",
        };

        public static List<string> HotfixAssemblyNames { get; } = new List<string>()
        {
            "App.Frame.dll",
            "App.Module.dll",
        };

        private byte[] _assemblyBytes;
        private AssetBundle _dllAssetBundle;

        private void Awake()
        {
        }

        private void LoadMetadataForAOTAssemblies()
        {
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in AOTMetaAssemblyNames)
            {
                TextAsset ta = _dllAssetBundle.LoadAsset<TextAsset>(aotDllName + ".bytes");
                byte[] dllBytes = ta.bytes;
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }
    }
}