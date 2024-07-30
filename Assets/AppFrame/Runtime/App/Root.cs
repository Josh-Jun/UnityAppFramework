using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Text;
using AppFrame.Config;
using AppFrame.Info;
using AppFrame.Interface;
using AppFrame.Manager;
using UnityEngine.SceneManagement;

namespace App
{
    public class Root
    {
        private static Dictionary<string, List<string>> sceneScriptsPairs = new Dictionary<string, List<string>>();
        private static Dictionary<string, ILogic> iLogicPairs = new Dictionary<string, ILogic>();
        private static List<ILogic> RuntimeLogics = new List<ILogic>();

        public static void Init()
        {
            //Log开关
            Log.Enabled = Global.AppConfig.IsDebug;
            //禁止程序休眠
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //设置程序帧率
            Application.targetFrameRate = Global.AppConfig.AppFrameRate;
            //输出App信息
            OutputAppInfo();
            //初始化Logic脚本
            InitLogicScripts();
            InitLogicBegin(Global.AppScene);
        }
        
        private static void OutputAppInfo()
        {
            var stringBuilder = new StringBuilder(1024);
            stringBuilder.AppendLine("App配置信息:");
            stringBuilder.AppendLine($"当前操作系统 : {SystemInfo.operatingSystem}");
            stringBuilder.AppendLine($"系统运行内存 : {SystemInfo.systemMemorySize/1000f}G");
            stringBuilder.AppendLine($"设备唯一标识 : {SystemInfo.deviceUniqueIdentifier}");
            var infoServer = Global.AppConfig.IsTestServer ? "是" : "否";
            stringBuilder.AppendLine($"是否测试环境 : {infoServer}");
            var mold = new []{ "原生资源", "本地资产", "远端资产" };
            stringBuilder.AppendLine($"资源加载模式 : {mold[(int)Global.AppConfig.LoadAssetsMold]}");
            var infoDevelopment = Global.AppConfig.IsDebug ? "是" : "否";
            stringBuilder.AppendLine($"是否开发构建 : {infoDevelopment}");
            stringBuilder.AppendLine($"应用默认帧率 : {Global.AppConfig.AppFrameRate}");
            stringBuilder.AppendLine($"资产构建管线 : {Global.AppConfig.ABPipeline}");
            stringBuilder.AppendLine($"资产版本标识 : {Global.AppConfig.ResVersion}");
            Log.I(stringBuilder.ToString());
        }

        public static void StartApp()
        {
            if (Global.AppConfig.LoadAssetsMold != LoadAssetsMold.Native)
            {
                AssetBundleManager.Instance.LoadAssetBundle("Shader", "Shaders");
            }
            //初始化Global的Logic脚本的Begin方法
            InitLogicBegin("Global");
            LoadScene(Global.AppScriptConfig.MainSceneName, true);
        }
        /// <summary>
        /// 初始化所有Logic脚本
        /// </summary>
        private static void InitLogicScripts()
        {
            for (int i = 0; i < Global.AppScriptConfig.LogicScript.Count; i++)
            {
                if (!iLogicPairs.ContainsKey(Global.AppScriptConfig.LogicScript[i].ScriptName))
                {
                    ILogic iLogic = GetLogic(Global.AppScriptConfig.LogicScript[i].ScriptName);
                    if (iLogic != null)
                    {
                        iLogicPairs.Add(Global.AppScriptConfig.LogicScript[i].ScriptName, iLogic);
                    }
                    else
                    {
                        Log.E($"Root脚本为空 脚本名称:{Global.AppScriptConfig.LogicScript[i].ScriptName}");
                    }
                }

                if (!sceneScriptsPairs.ContainsKey(Global.AppScriptConfig.LogicScript[i].SceneName))
                {
                    List<string> scripts = new List<string>();
                    scripts.Add(Global.AppScriptConfig.LogicScript[i].ScriptName);
                    sceneScriptsPairs.Add(Global.AppScriptConfig.LogicScript[i].SceneName, scripts);
                }
                else
                {
                    sceneScriptsPairs[Global.AppScriptConfig.LogicScript[i].SceneName]
                        .Add(Global.AppScriptConfig.LogicScript[i].ScriptName);
                }
            }
        }
        /// <summary>
        /// 加载场景（只能通过这个方法加载场景，否则Logic脚本不能正常实用Begin和End方法）
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="isLoading"></param>
        /// <param name="loadingEvent"></param>
        /// <param name="mode"></param>
        public static void LoadScene(string sceneName, bool isLoading = false, Action<float> loadingEvent = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
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
                    InitLogicBegin(sceneName);
                    loadingEvent?.Invoke(1);
                }, mode);
            }
        }
        /// <summary>
        /// 根据场景名称执行End和Begin方法
        /// </summary>
        /// <param name="sceneName"></param>
        private static void InitLogicBegin(string sceneName)
        {
            // 先执行当前Logic脚本的End方法
            foreach (var logic in RuntimeLogics)
            {
                logic.End();
            }
            // 清理运行时Logic脚本
            RuntimeLogics.Clear();
            // 根据加载的场景，找到当前场景所有的运行时Logic，并执行Begin方法
            if (sceneScriptsPairs.ContainsKey(sceneName))
            {
                for (int i = 0; i < sceneScriptsPairs[sceneName].Count; i++)
                {
                    if (iLogicPairs.ContainsKey(sceneScriptsPairs[sceneName][i]))
                    {
                        iLogicPairs[sceneScriptsPairs[sceneName][i]].Begin();
                        if (sceneName.Equals("Global")) continue;
                        RuntimeLogics.Add(iLogicPairs[sceneScriptsPairs[sceneName][i]]);
                    }
                }
            }
        }
        /// <summary>
        /// 根据脚本名称，在程序集中获取实例
        /// </summary>
        /// <param name="fullName"></param>
        /// <param name="assemblyString"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取对应Logic脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
            if (Global.AppScriptConfig != null)
            {
                for (int i = 0; i < Global.AppScriptConfig.LogicScript.Count; i++)
                {
                    if (iLogicPairs.ContainsKey(Global.AppScriptConfig.LogicScript[i].ScriptName))
                    {
                        iLogicPairs[Global.AppScriptConfig.LogicScript[i].ScriptName].AppPause(isPause);
                    }
                }
            }
        }

        public static void AppFocus(bool isFocus)
        {
            if (Global.AppScriptConfig != null)
            {
                for (int i = 0; i < Global.AppScriptConfig.LogicScript.Count; i++)
                {
                    if (iLogicPairs.ContainsKey(Global.AppScriptConfig.LogicScript[i].ScriptName))
                    {
                        iLogicPairs[Global.AppScriptConfig.LogicScript[i].ScriptName].AppFocus(isFocus);
                    }
                }
            }
        }

        public static void AppQuit()
        {
            if (Global.AppScriptConfig != null)
            {
                for (int i = 0; i < Global.AppScriptConfig.LogicScript.Count; i++)
                {
                    if (iLogicPairs.ContainsKey(Global.AppScriptConfig.LogicScript[i].ScriptName))
                    {
                        iLogicPairs[Global.AppScriptConfig.LogicScript[i].ScriptName].AppQuit();
                    }
                }
            }
        }
    }
}