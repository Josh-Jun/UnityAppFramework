using System;
using System.Collections;
using System.Collections.Generic;
using AppFrame.Config;
using HybridCLR;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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

        public static AppScriptConfig AppScriptConfig;
        public static AppConfig AppConfig;
        public static GameObject UpdateView;
        private void Awake()
        {
            AppConfig = Resources.Load<AppConfig>("AppConfig");

            AssetBundleManager.Instance.InitManager();
            
            HotFix.Init((isHotfix) =>
            {
                StartCoroutine(DownLoading(isHotfix));
            });
            
        }
        
        private IEnumerator DownLoading(bool isHotfix)
        {
            if (isHotfix)
            {
                float time = 0;
                float previousSize = 0;
                float speed = 0;
                while (HotFix.GetProgress < 1)
                {
                    yield return new WaitForEndOfFrame();
                    time += Time.deltaTime;
                    if (time >= 1f)
                    {
                        speed = (HotFix.GetLoadedSize - previousSize);
                        previousSize = HotFix.GetLoadedSize;
                        time = 0;
                    }

                    speed = speed > 0 ? speed : 0;
                    //set progress 0-1
                }
                yield return new WaitForEndOfFrame();
                //set progress 1
                yield return new WaitForSeconds(0.2f);
            }
            //更新下载完成，开始运行App
            
            Load(isHotfix);
        }

        private void Load(bool isHotfix)
        {
            if (isHotfix)
            {
                LoadMetadataForAOTAssemblies();
                LoadHotfixAssemblies();
                AppScriptConfig = AssetBundleManager.Instance.LoadAsset<AppScriptConfig>("App", "Config", "AppScriptConfig");
                AssetBundle ab = AssetBundleManager.Instance.LoadAssetBundle("Scenes", "AppScene");
                UpdateView = AssetBundleManager.Instance.LoadAsset<GameObject>("App", "Update", "UpdateView");
            }
            else
            {
                AppScriptConfig = Resources.Load<AppScriptConfig>("HybridFolder/App/Config/AppScriptConfig");
            }
            
            SceneManager.LoadScene("AppScene");
        }

        private void LoadHotfixAssemblies()
        {
            for (int i = 0; i < HotfixAssemblyNames.Count; i++)
            {
                string name = HotfixAssemblyNames[i];
                TextAsset ta = AssetBundleManager.Instance.LoadAsset<TextAsset>("App", "Dll", name);
                System.Reflection.Assembly.Load(ta.bytes);
            }
        }
        private void LoadMetadataForAOTAssemblies()
        {
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in AOTMetaAssemblyNames)
            {
                TextAsset ta =  AssetBundleManager.Instance.LoadAsset<TextAsset>("App", "Dll", aotDllName);
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(ta.bytes, mode);
            }
        }
    }
}