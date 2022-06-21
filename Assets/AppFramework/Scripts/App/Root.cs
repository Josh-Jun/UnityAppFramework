using Update;
using System;
using System.Collections.Generic;
using UnityEngine;
using XLuaFrame;
using System.Collections;
using UnityEngine.XR.Management;

public class Root
{
    private static IRoot UpdateRoot = null;
    private static IRoot AskRoot = null;

    private const string path_AppScriptConfig = "App/Assets/AppScriptConfig";
    private static AppScriptConfig appScriptConfig;

    private const string path_AppConfig = "App/AppConfig";
    public static AppConfig AppConfig;//App配置表 

    private static Dictionary<string, List<string>> sceneScriptsPairs = new Dictionary<string, List<string>>();
    private static Dictionary<string, IRoot> iRootPairs = new Dictionary<string, IRoot>();

    public static LoadingScene LoadingScene { private set; get; }
    public static void Init()
    {
        AppConfig = Resources.Load<AppConfig>(path_AppConfig);
        //Debug.Log开关
        Debug.unityLogger.logEnabled = AppConfig.IsDebug;
        //禁止程序休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //设置程序帧率
        Application.targetFrameRate = AppConfig.AppFrameRate;

        OutputCfgInfo();

        BeginCheckUpdate();
    }

    private static void OutputCfgInfo()
    {
        string info_server = AppConfig.IsProServer ? "生产环境" : "测试环境";
        Debug.Log("配置信息:");
        Debug.Log($"当前服务器:{info_server}");
    }

    private static void BeginCheckUpdate()
    {
        if (AppConfig.IsHotfix && AppConfig.IsLoadAB)
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
        InitRootScripts(() => { LoadScene(appScriptConfig.MainSceneName, true); InitManager(); });
    }
    private static void InitManager()
    {
        AssetBundleManager.Instance.InitManager();
        AssetsManager.Instance.InitManager();
        AudioManager.Instance.InitManager();
        AVProManager.Instance.InitManager();
        VideoManager.Instance.InitManager();
        TableManager.Instance.InitManager();
    }
    private static void InitRootScripts(Action callback = null)
    {
        AskRoot = GetRoot("Ask.AskRoot");
        AskRoot.Begin();
        TextAsset config = AssetsManager.Instance.LoadAsset<TextAsset>(path_AppScriptConfig);
        appScriptConfig = XmlSerializeManager.ProtoDeSerialize<AppScriptConfig>(config.bytes);
        for (int i = 0; i < appScriptConfig.RootScript.Count; i++)
        {
            IRoot iRoot;
            //判断是否存在XLua脚本，如果存在，执行XLua代码，不存在执行C#代码
            if (AppConfig.RunXLua && XLuaManager.Instance.IsLuaFileExist(appScriptConfig.RootScript[i].LuaScriptPath))
            {
                XLuaRoot root = new XLuaRoot();
                root.Init(appScriptConfig.RootScript[i].LuaScriptPath);
                iRoot = root;
            }
            else
            {
                iRoot = GetRoot(appScriptConfig.RootScript[i].ScriptName);
            }
            if (!iRootPairs.ContainsKey(appScriptConfig.RootScript[i].ScriptName))
            {
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
                sceneScriptsPairs[appScriptConfig.RootScript[i].SceneName].Add(appScriptConfig.RootScript[i].ScriptName);
            }
        }
        callback?.Invoke();
    }
    public static void LoadScene(string sceneName, bool isLoading = false, Action callback = null)
    {
        if (isLoading)
        {
            LoadingScene = new LoadingScene(sceneName, callback);
            LoadScene("LoadingScene");
        }
        else
        {
            AssetsManager.Instance.LoadSceneAsync(sceneName, (AsyncOperation async) =>
            {
                if (async.isDone && async.progress == 1)
                {
                    InitRootBegin(sceneName, callback);
                }
            });
        }
    }

    public static void InitRootBegin(string sceneName, Action callback = null)
    {
        AskRoot.Begin();
        if (sceneScriptsPairs.ContainsKey(sceneName))
        {
            for (int i = 0; i < sceneScriptsPairs[sceneName].Count; i++)
            {
                if (iRootPairs.ContainsKey(sceneScriptsPairs[sceneName][i]))
                {
                    iRootPairs[sceneScriptsPairs[sceneName][i]].Begin();
                }
            }
        }
        callback?.Invoke();
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
    public static void End()
    {
        UpdateRoot?.End();
        for (int i = 0; i < iRootPairs.Count; i++)
        {
            iRootPairs[appScriptConfig.RootScript[i].ScriptName].End();
        }
    }
}
public struct LoadingScene
{
    public string Name;
    public Action Callback;
    public LoadingScene(string name, Action callback)
    {
        this.Name = name;
        this.Callback = callback;
    }
}