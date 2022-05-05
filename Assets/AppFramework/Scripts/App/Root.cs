using Update;
using System;
using System.Collections.Generic;
using UnityEngine;
using XLuaFrame;
using System.Collections;
using UnityEngine.XR.Management;

public class Root
{
    private const string path_AppConfig = "App/AppConfig";
    public static AppConfig AppConfig;//App配置表 

    private const string path_AppScriptConfig = "App/Assets/AppScriptConfig";
    private static AppScriptConfig appScriptConfig;

    private static readonly Dictionary<string, List<string>> sceneScriptsPairs = new Dictionary<string, List<string>>();
    private static readonly Dictionary<string, IRoot> iRootPairs = new Dictionary<string, IRoot>();
    private static IRoot UpdateRoot = null;
    public static void Init()
    {
        AppConfig = Resources.Load<AppConfig>(path_AppConfig);
        //Debug.Log开关
        Debug.unityLogger.logEnabled = AppConfig.IsDebug;
        //禁止程序休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        //设置程序帧率
        Application.targetFrameRate = AppConfig.AppFrameRate;

        BeginCheckUpdate();
    }
    private static void BeginCheckUpdate()
    {
        Debug.Log("BeginCheckUpdate");
        if (AppConfig.IsHotfix || AppConfig.IsLoadAB)
        {
            //初始化热更脚本
            Type type = Type.GetType("Update.UpdateRoot");
            object obj = Activator.CreateInstance(type);
            UpdateRoot = obj as IRoot;
            UpdateRoot.Begin();
        }
        else
        {
            InitRootScripts();
        }
    }

    public static void InitRootScripts()
    {
        Debug.Log("InitRootScripts");
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
                iRoot = root as IRoot;
            }
            else
            {
                Type type = Type.GetType(appScriptConfig.RootScript[i].ScriptName);
                object obj = Activator.CreateInstance(type);
                iRoot = obj as IRoot;
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
        LoadScene(appScriptConfig.MainSceneName);
    }
    public static void LoadScene(string sceneName, Action callback = null)
    {
        AssetsManager.Instance.LoadSceneAsync(sceneName, (AsyncOperation async) =>
        {
            if (async.isDone && async.progress == 1)
            {
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
        });
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
