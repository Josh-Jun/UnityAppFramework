using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using AppFrame.Attribute;
using AppFrame.Interface;
using AppFrame.Manager;
using AppFrame.Tools;
using AppFrame.Config;
using AppFrame.View;
using UnityEngine.SceneManagement;

namespace App
{
    public class Root
    {
        public const string AppScene = "Scenes/App/AppScene";
        private static Dictionary<string, List<ILogic>> SceneLogicPairs = new Dictionary<string, List<ILogic>>();
        private static string CurrentSceneName;

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
            CurrentSceneName = AppScene;
            ExecuteSceneMethods(CurrentSceneName, "Begin");
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
            ViewManager.Instance.InitViewScripts();
            //初始化Global的Logic脚本的Begin方法
            ExecuteSceneMethods("Global", "Begin");
            LoadScene(Assets.MainScene);
        }
        /// <summary>
        /// 初始化所有Logic脚本
        /// </summary>
        private static void InitLogicScripts()
        {
            var types = Utils.GetAssemblyTypes<ILogic>();
            foreach (var type in types)
            {
                var la = type.GetCustomAttributes(typeof(LogicOfAttribute), false).First();
                if (la is not LogicOfAttribute attribute) continue;
                var obj = Activator.CreateInstance(type);
                var logic = obj as ILogic;
                if (!SceneLogicPairs.ContainsKey(attribute.Scene))
                {
                    var logics = new List<ILogic> { logic };
                    SceneLogicPairs.Add(attribute.Scene, logics);
                }
                else
                {
                    SceneLogicPairs[attribute.Scene].Add(logic);
                }
            }
        }
        /// <summary>
        /// 加载场景（只能通过这个方法加载场景，否则Logic脚本不能正常实用Begin和End方法）
        /// </summary>
        /// <param name="targetSceneName"></param>
        /// <param name="isLoading"></param>
        /// <param name="loadingEvent"></param>
        /// <param name="mode"></param>
        public static void LoadScene(string targetSceneName, bool isLoading = false, Action<float> loadingEvent = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            ExecuteSceneMethods(CurrentSceneName, "End");
            CurrentSceneName = targetSceneName;
            if (isLoading)
            {
                AssetsManager.Instance.LoadingSceneAsync(targetSceneName, (progress) => 
                { 
                    loadingEvent?.Invoke(progress);
                    if (progress >= 1)
                    {
                        ExecuteSceneMethods(CurrentSceneName, "Begin");
                    }
                }, mode);
            }
            else
            {
                AssetsManager.Instance.LoadSceneAsync(targetSceneName, () =>
                {
                    ExecuteSceneMethods(CurrentSceneName, "Begin");
                    loadingEvent?.Invoke(1);
                }, mode);
            }
        }
        /// <summary>
        /// 获取对应Logic脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetLogicScript<T>() where T : class
        {
            var type = typeof(T);
            foreach (var logics in SceneLogicPairs.Values)
            {
                if (logics.FirstOrDefault(logic => logic.GetType() == type) is T t) return t;
            }
            return null;
        }

        private static void ExecuteSceneMethods(string sceneName, string methodName, params object[] args)
        {
            var types = Utils.GetObjsType(args);
            if (!SceneLogicPairs.TryGetValue(sceneName, value: out var pair)) return;
            foreach (var logic in pair)
            {
                var method = logic.GetType().GetMethod(methodName, types);
                method?.Invoke(logic, args);
            }
        }

        private static void ExecuteMethods(string methodName, params object[] args)
        {
            var types = Utils.GetObjsType(args);
            foreach (var logics in SceneLogicPairs.Values)
            {
                foreach (var logic in logics)
                {
                    var method = logic.GetType().GetMethod(methodName, types);
                    method?.Invoke(logic, args);
                }
            }
        }
        
        public static void AppPause(bool isPause)
        {
            ExecuteMethods("AppPause", isPause);
        }

        public static void AppFocus(bool isFocus)
        {
            ExecuteMethods("AppFocus", isFocus);
        }

        public static void AppQuit()
        {
            ExecuteMethods("AppQuit");
        }
    }
}