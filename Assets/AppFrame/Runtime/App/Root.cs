using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Text;
using AppFrame.Config;
using AppFrame.Data;
using AppFrame.Enum;
using AppFrame.Info;
using AppFrame.Interface;
using AppFrame.Manager;
using AppFrame.Tools;
using AppFrame.Data;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ULauncher = Launcher.Launcher;

namespace App
{
    public class Root
    {
        private static ILogic _updateLogic = null;

        private static AppScriptConfig appScriptConfig;

        private static Dictionary<string, List<string>> sceneScriptsPairs = new Dictionary<string, List<string>>();
        private static Dictionary<string, ILogic> iLogicPairs = new Dictionary<string, ILogic>();
        private static List<ILogic> RuntimeRoots = new List<ILogic>();

        public static void Init()
        {
            AppInfo.AppConfig = ULauncher.AppConfig;
            
            //Log开关
            Log.Enabled = AppInfo.AppConfig.IsDebug;
            //禁止程序休眠
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //设置程序帧率
            Application.targetFrameRate = AppInfo.AppConfig.AppFrameRate;
            //输出App信息
            OutputAppInfo();
            //初始化Logic脚本
            InitLogicScripts();
            InitLogicBegin(AssetsPathConfig.AppScene);
        }
        
        private static void OutputAppInfo()
        {
            var stringBuilder = new StringBuilder(1024);
            stringBuilder.AppendLine("App配置信息:");
            stringBuilder.AppendLine($"操作系统 : {SystemInfo.operatingSystem}");
            stringBuilder.AppendLine($"系统内存 : {SystemInfo.systemMemorySize/1000f}G");
            stringBuilder.AppendLine($"设备标识 : {SystemInfo.deviceUniqueIdentifier}");
            stringBuilder.AppendLine("--------------------------------------------------");
            var infoServer = AppInfo.AppConfig.IsTestServer ? "是" : "否";
            stringBuilder.AppendLine($"是否测试环境 : {infoServer}");
            var mold = new []{ "原生资源", "本地资产", "远端资产" };
            stringBuilder.AppendLine($"资源加载模式 : {mold[(int)AppInfo.AppConfig.LoadAssetsMold]}");
            var infoDevelopment = AppInfo.AppConfig.IsDebug ? "是" : "否";
            stringBuilder.AppendLine($"是否开发构建 : {infoDevelopment}");
            stringBuilder.AppendLine($"应用默认帧率 : {AppInfo.AppConfig.AppFrameRate}");
            stringBuilder.AppendLine($"资产构建管线 : {AppInfo.AppConfig.ABPipeline}");
            stringBuilder.AppendLine($"资产版本标识 : {AppInfo.AppConfig.ResVersion}");
            Log.I(stringBuilder.ToString());
        }

        public static void StartApp()
        {
            AssetBundleManager.Instance.LoadAssetBundle("Shader", "Shaders");
            //初始化Global的Logic脚本的Begin方法
            InitLogicBegin("Global");
            LoadScene(appScriptConfig.MainSceneName, true);
        }
        private static void InitLogicScripts()
        {
            appScriptConfig = ULauncher.AppScriptConfig;
            for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
            {
                if (!iLogicPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
                {
                    ILogic iLogic = GetLogic(appScriptConfig.RootScript[i].ScriptName);
                    if (iLogic != null)
                    {
                        iLogicPairs.Add(appScriptConfig.RootScript[i].ScriptName, iLogic);
                    }
                    else
                    {
                        Debug.LogError($"Root脚本为空 脚本名称:{appScriptConfig.RootScript[i].ScriptName}");
                    }
                }

                if (!sceneScriptsPairs.ContainsKey(appScriptConfig.RootScript[i].SceneName))
                {
                    List<string> scripts = new List<string>();
                    scripts.Add(appScriptConfig.RootScript[i].ScriptName);
                    sceneScriptsPairs.Add(appScriptConfig.RootScript[i].SceneName, scripts);
                }
                else
                {
                    sceneScriptsPairs[appScriptConfig.RootScript[i].SceneName]
                        .Add(appScriptConfig.RootScript[i].ScriptName);
                }
            }
        }

        public static void LoadScene(string sceneName, bool isLoading = false, Action<float> loadingEvent = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            InitLogicEnd();
            if (isLoading)
            {
                AssetsManager.Instance.LoadingSceneAsync(sceneName, (progress) => 
                { 
                    loadingEvent?.Invoke(progress);
                    if (progress >= 1)
                    {
                        InitLogicBegin(sceneName);
                    }
                }, mode);
            }
            else
            {
                AssetsManager.Instance.LoadSceneAsync(sceneName, () =>
                {
                    InitLogicBegin(sceneName, () => 
                    {
                        loadingEvent?.Invoke(1);  
                    });
                }, mode);
            }
        }

        public static void InitLogicBegin(string sceneName, Action callback = null)
        {
            if (sceneScriptsPairs.ContainsKey(sceneName))
            {
                for (int i = 0; i < sceneScriptsPairs[sceneName].Count; i++)
                {
                    if (iLogicPairs.ContainsKey(sceneScriptsPairs[sceneName][i]))
                    {
                        iLogicPairs[sceneScriptsPairs[sceneName][i]].Begin();
                        if (sceneName.Equals("Global")) continue;
                        RuntimeRoots.Add(iLogicPairs[sceneScriptsPairs[sceneName][i]]);
                    }
                }
            }

            callback?.Invoke();
        }

        private static void InitLogicEnd()
        {
            for (int i = 0; i < RuntimeRoots.Count; i++)
            {
                RuntimeRoots[i].End();
            }

            RuntimeRoots.Clear();
        }

        private static ILogic GetLogic(string fullName, string assemblyString = "App.Module")
        {
            if (!AppInfo.AssemblyPairs.ContainsKey(assemblyString))
            {
                AppInfo.AssemblyPairs.Add(assemblyString, Assembly.Load(assemblyString));//加载程序集,返回类型是一个Assembly
            }
            Type type = AppInfo.AssemblyPairs[assemblyString].GetType($"{fullName}");
            var obj = Activator.CreateInstance(type); //创建此类型实例
            return obj as ILogic;
        }

        public static T GetLogicScript<T>() where T : class
        {
            var type = typeof(T);
            var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
            if (!iLogicPairs.ContainsKey(scriptName))
            {
                return null;
            }

            return iLogicPairs[scriptName] as T;
        }

        public static void AppPause(bool isPause)
        {
            _updateLogic?.AppPause(isPause);
            if (appScriptConfig != null)
            {
                for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
                {
                    if (iLogicPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
                    {
                        iLogicPairs[appScriptConfig.RootScript[i].ScriptName].AppPause(isPause);
                    }
                }
            }
        }

        public static void AppFocus(bool isFocus)
        {
            _updateLogic?.AppFocus(isFocus);
            if (appScriptConfig != null)
            {
                for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
                {
                    if (iLogicPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
                    {
                        iLogicPairs[appScriptConfig.RootScript[i].ScriptName].AppFocus(isFocus);
                    }
                }
            }
        }

        public static void AppQuit()
        {
            _updateLogic?.AppQuit();
            if (appScriptConfig != null)
            {
                for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
                {
                    if (iLogicPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
                    {
                        iLogicPairs[appScriptConfig.RootScript[i].ScriptName].AppQuit();
                    }
                }
            }
        }
    }
}