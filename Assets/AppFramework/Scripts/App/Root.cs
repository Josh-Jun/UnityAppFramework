using Update;
using System;
using System.Collections.Generic;
using UnityEngine;
using XLuaFrame;
using System.Collections;
using Loading;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;

public class Root
{
    private static IRoot UpdateRoot = null;

    private const string path_AppScriptConfig = "App/Config/AppScriptConfig";
    private static AppScriptConfig appScriptConfig;

    private const string path_AppConfig = "App/AppConfig";
    public static AppConfig AppConfig; //App配置表 

    private static Dictionary<string, List<string>> sceneScriptsPairs = new Dictionary<string, List<string>>();
    private static Dictionary<string, IRoot> iRootPairs = new Dictionary<string, IRoot>();
    private static List<IRoot> RuntimeRoots = new List<IRoot>();

    public static void Init()
    {
        AppConfig = Resources.Load<AppConfig>(path_AppConfig);
        //Debug.Log开关
        Debug.unityLogger.logEnabled = AppConfig.IsDebug;
        //禁止程序休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //设置程序帧率
        Application.targetFrameRate = AppConfig.AppFrameRate;

        InitCamera();
        InitManager();
        OutputAppInfo();
        BeginCheckUpdate();
    }
    /// <summary> 初始化App场景相机，热更新在这个场景执行 </summary>
    private static void InitCamera()
    {
        string name = AppConfig.TargetPackage == TargetPackage.Pico ? "PicoXRManager" : "Main Camera";
        GameObject go = Resources.Load<GameObject>($"App/Camera/{name}");
        GameObject camera = GameObject.Instantiate(go);
        camera.name = name;
    }
    private static void OutputAppInfo()
    {
        string info_server = AppConfig.IsTestServer ? "测试环境" : "生产环境";
        Debug.Log("配置信息:");
        Debug.Log($"当前服务器:{info_server}");
    }

    private static void BeginCheckUpdate()
    {
        if (AppConfig.IsHotfix)
        {
            //初始化热更脚本
            UpdateRoot = GetRoot("Update.UpdateRoot");
            UpdateRoot.Begin();
        }
        else
        {
            StartApp();
        }
    }

    public static void StartApp()
    {
        InitRootScripts(() =>
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

        PlatformMsgReceiver.Instance.InitManager(parent);
        AssetBundleManager.Instance.InitManager(parent);
        TimerTaskManager.Instance.InitManager(parent);
        AssetsManager.Instance.InitManager(parent);
        NetcomManager.Instance.InitManager(parent);
        TimerManager.Instance.InitManager(parent);
        AudioManager.Instance.InitManager(parent);
        AVProManager.Instance.InitManager(parent);
        VideoManager.Instance.InitManager(parent);
        TableManager.Instance.InitManager(parent);
    }

    private static void InitRootScripts(Action callback = null)
    {
        appScriptConfig = AssetsManager.Instance.LoadAsset<AppScriptConfig>(path_AppScriptConfig);
        for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
        {
            if (!iRootPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
            {
                IRoot iRoot;
                //判断是否存在XLua脚本，如果存在，执行XLua代码，不存在执行C#代码
                if (AppConfig.RunXLua &&
                    XLuaManager.Instance.IsLuaFileExist(appScriptConfig.RootScript[i].LuaScriptPath))
                {
                    XLuaRoot root = new XLuaRoot();
                    root.Init(appScriptConfig.RootScript[i].LuaScriptPath);
                    iRoot = root;
                }
                else
                {
                    iRoot = GetRoot(appScriptConfig.RootScript[i].ScriptName);
                }

                if (iRoot != null)
                {
                    iRootPairs.Add(appScriptConfig.RootScript[i].ScriptName, iRoot);
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

        InitRootBegin("Global", callback);
    }

    public static void LoadScene(string sceneName, bool isLoading = false, LoadSceneMode mode = LoadSceneMode.Single, Action callback = null)
    {
        InitRootEnd();
        if (isLoading)
        {
            AssetsManager.Instance.LoadingSceneAsync(sceneName, (progress) =>
            {
                GetRootScript<LoadingRoot>().Loading(progress, () =>
                {
                    InitRootBegin(sceneName, callback);
                });
            });
        }
        else
        {
            AssetsManager.Instance.LoadSceneAsync(sceneName, () =>
            {
                InitRootBegin(sceneName, callback);
            }, mode);
        }
    }

    public static void InitRootBegin(string sceneName, Action callback = null)
    {
        if (sceneScriptsPairs.ContainsKey(sceneName))
        {
            for (int i = 0; i < sceneScriptsPairs[sceneName].Count; i++)
            {
                if (iRootPairs.ContainsKey(sceneScriptsPairs[sceneName][i]))
                {
                    iRootPairs[sceneScriptsPairs[sceneName][i]].Begin();
                    if (sceneName.Equals("Global")) continue;
                    RuntimeRoots.Add(iRootPairs[sceneScriptsPairs[sceneName][i]]);
                }
            }
        }

        callback?.Invoke();
    }

    private static void InitRootEnd()
    {
        for (int i = 0; i < RuntimeRoots.Count; i++)
        {
            RuntimeRoots[i].End();
        }

        RuntimeRoots.Clear();
    }

    private static IRoot GetRoot(string fullName)
    {
        Type type = Type.GetType(fullName);
        object obj = Activator.CreateInstance(type);
        return obj as IRoot;
    }

    public static T GetRootScript<T>() where T : class
    {
        var type = typeof(T);
        var scriptName = type.Namespace == string.Empty ? type.Name : type.FullName;
        if (!iRootPairs.ContainsKey(scriptName))
        {
            return null;
        }

        return iRootPairs[scriptName] as T;
    }

    public static void AppPause(bool isPause)
    {
        UpdateRoot?.AppPause(isPause);
        if (appScriptConfig != null)
        {
            for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
            {
                if (iRootPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
                {
                    iRootPairs[appScriptConfig.RootScript[i].ScriptName].AppPause(isPause);
                }
            }
        }
    }

    public static void AppFocus(bool isFocus)
    {
        UpdateRoot?.AppFocus(isFocus);
        if (appScriptConfig != null)
        {
            for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
            {
                if (iRootPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
                {
                    iRootPairs[appScriptConfig.RootScript[i].ScriptName].AppFocus(isFocus);
                }
            }
        }
    }

    public static void AppQuit()
    {
        UpdateRoot?.AppQuit();
        if (appScriptConfig != null)
        {
            for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
            {
                if (iRootPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
                {
                    iRootPairs[appScriptConfig.RootScript[i].ScriptName].AppQuit();
                }
            }
        }
    }
}