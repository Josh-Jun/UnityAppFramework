using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Reflection;
using AppFramework.Config;
using AppFramework.Data;
using AppFramework.Enum;
using AppFramework.Info;
using AppFramework.Interface;
using AppFramework.Manager;
using AppFramework.Tools;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
            AppInfo.AppConfig = Resources.Load<AppConfig>("App/AppConfig");
            
            //Debug.Log开关
            Debug.unityLogger.logEnabled = AppInfo.AppConfig.IsDebug;
            //禁止程序休眠
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //设置程序帧率
            Application.targetFrameRate = AppInfo.AppConfig.AppFrameRate;

            InitCamera();
            InitManager();
            OutputAppInfo();
            BeginCheckUpdate();
        }

        /// <summary> 初始化App场景相机，热更新在这个场景执行 </summary>
        private static void InitCamera()
        {
            string name = AppInfo.AppConfig.TargetPackage == TargetPackage.XR ? "PicoXRManager" : "Main Camera";
            GameObject go = Resources.Load<GameObject>($"App/Camera/{name}");
            GameObject camera = GameObject.Instantiate(go);
            camera.name = name;
        }

        private static void OutputAppInfo()
        {
            string info_server = AppInfo.AppConfig.IsTestServer ? "测试环境" : "生产环境";
            Debug.Log("配置信息:");
            Debug.Log($"当前服务器:{info_server}");
        }

        private static void BeginCheckUpdate()
        {
            if (AppInfo.AppConfig.IsHotfix)
            {
                //初始化热更脚本
                _updateLogic = GetLogic("App.Update.UpdateRoot");
                _updateLogic.Begin();
            }
            else
            {
                StartApp();
            }
        }

        public static void StartApp()
        {
            InitLogicScripts(() =>
            {
                LoadScene(appScriptConfig.MainSceneName, true);
                TableManager.Instance.InitConfig();
            });
        }

        private static void InitManager()
        {
            GameObject go = new GameObject("Manager");
            Transform parent = go.transform;
            parent.SetParent(App.app.transform);

            PlatformMsgReceiver.Instance.InitParent(parent);
            AssetBundleManager.Instance.InitParent(parent);
            AssetsManager.Instance.InitParent(parent);
            NetcomManager.Instance.InitParent(parent);
            AudioManager.Instance.InitParent(parent);
            VideoManager.Instance.InitParent(parent);
            TableManager.Instance.InitParent(parent);
            TimerTasker.Instance.InitParent(parent);
            TimerLogicer.Instance.InitParent(parent);
        }

        private static void InitLogicScripts(Action callback = null)
        {
            appScriptConfig = AssetsManager.Instance.LoadAsset<AppScriptConfig>(AssetsPathConfig.AppScriptConfig);
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

            InitLogicBegin("Global", callback);
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

        private static ILogic GetLogic(string fullName)
        {
            if (AppInfo.Assembly == null)
            {
                AppInfo.Assembly = Assembly.Load("App.Module");//加载程序集,返回类型是一个Assembly
            }
            Type type = AppInfo.Assembly.GetType($"{fullName}");
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