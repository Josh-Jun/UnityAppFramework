using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine.SceneManagement;

namespace App.Core
{
    public abstract class Root
    {
        private static readonly Dictionary<string, List<ILogic>> SceneLogicPairs = new Dictionary<string, List<ILogic>>();
        private static string CurrentScene;

        public static void Init()
        {
            // Log开关
            Log.Enabled = Global.AppConfig.EnableLog;
            // 禁止程序休眠
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            // 设置程序帧率
            Application.targetFrameRate = Global.AppConfig.AppFrameRate;
            // 输出App信息
            OutputAppInfo();
            // 加载所有配置表
            AssetsMaster.Instance.LoadConfigs();
            // 初始化Logic脚本
            InitLogicScripts();
            // 初始化AppScene的所有Logic的Begin方法
            CurrentScene = AssetPath.AppScene;
            ExecuteSceneMethods(CurrentScene, "Begin");
        }
        
        private static void OutputAppInfo()
        {
            var stringBuilder = new StringBuilder(1024);
            stringBuilder.AppendLine("App配置信息:");
            stringBuilder.AppendLine($"操作系统 : {SystemInfo.operatingSystem}");
            stringBuilder.AppendLine($"运行内存 : {SystemInfo.systemMemorySize/1000f}G");
            stringBuilder.AppendLine($"设备标识 : {SystemInfo.deviceUniqueIdentifier}");
            var enable = Global.AppConfig.EnableLog ? "开" : "关";
            stringBuilder.AppendLine($"日志开关 : {enable}");
            stringBuilder.AppendLine($"开发环境 : {Global.AppConfig.DevelopmentMold}");
            stringBuilder.AppendLine($"运行模式 : {Global.AppConfig.AssetPlayMode}");
            stringBuilder.AppendLine($"默认帧率 : {Global.AppConfig.AppFrameRate}");
            stringBuilder.AppendLine($"资源版本 : {Global.AppConfig.CDNVersion}");
            Log.I(stringBuilder.ToString());
        }

        public static void StartApp()
        {
            // 加载所有view
            ViewMaster.Instance.InitViewScripts();
            // 初始化Global的Logic脚本的Begin方法
            ExecuteSceneMethods(AssetPath.Global, "Begin");
            LoadScene(AssetPath.MainScene);
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
                if (!SceneLogicPairs.TryGetValue(attribute.Scene, out var pair))
                {
                    var logics = new List<ILogic> { logic };
                    SceneLogicPairs.Add(attribute.Scene, logics);
                }
                else
                {
                    pair.Add(logic);
                }
            }
        }
        /// <summary>
        /// 加载场景（只能通过这个方法加载场景，否则Logic脚本不能正常实用Begin和End方法）
        /// </summary>
        /// <param name="targetScene"></param>
        /// <param name="loadingEvent"></param>
        /// <param name="mode"></param>
        public static void LoadScene(string targetScene, Action<float> loadingEvent = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            ExecuteSceneMethods(CurrentScene, "End");
            CurrentScene = targetScene;
            var handle = Assets.LoadSceneAsync(targetScene, AssetPackage.HotfixPackage, mode);
            var time_id = TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                if (handle == null) return;
                loadingEvent?.Invoke(handle.Progress);
            });
            handle.Completed += sceneHandle =>
            {
                TimeUpdateMaster.Instance.EndTimer(time_id);
                ExecuteSceneMethods(CurrentScene, "Begin");
                loadingEvent?.Invoke(1);
            };
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