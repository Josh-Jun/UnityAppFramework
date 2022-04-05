using Hotfix;
using System;
using System.Collections.Generic;
using UnityEngine;
using XLuaFrame;

public class Root
{
    public const string MainSceneName = "TestScene";
    public const string path_AppRootConfig = "App/Assets/AppRootConfig";
    public const string path_debugConfig = "App/Debug/AppRootConfig";
    public static bool IsDebug { get; private set; } = false;//是否Log
    public static bool IsHotfix { get; private set; } = false;//是否热更
    public static bool IsLoadAB { get; private set; } = false;//是否加载AB包
    public static bool RunXLuaScripts { get; private set; } = true;//是否运行XLua脚本

    private static readonly Dictionary<string, List<string>> sceneScriptsPairs = new Dictionary<string, List<string>>();
    private static readonly string hotfixScriptName = "Hotfix.HotfixRoot";
    private static readonly Dictionary<string, IRoot> iRootPairs = new Dictionary<string, IRoot>();
    private static RootScriptConfig rootConfig;
    private static DebugConfig debugConfig;
    private static IRoot HotfixRoot = null;
    public static void Init()
    {
        TextAsset config = Resources.Load<TextAsset>(path_debugConfig);
        debugConfig = XmlSerializeManager.ProtoDeSerialize<DebugConfig>(config.bytes);
        IsDebug = debugConfig.IsDebug;
        Debuger.Init(IsDebug);

        if (IsHotfix)
        {
            //初始化热更脚本
            Type type = Type.GetType(hotfixScriptName);
            object obj = Activator.CreateInstance(type);
            HotfixRoot = obj as IRoot;
            HotfixRoot.Begin();
        }
        else
        {
            InitRootScripts(()=> { LoadScene(MainSceneName); });
        }
    }

    public static void InitRootScripts(Action callback = null)
    {
        TextAsset config = AssetsManager.Instance.LoadAsset<TextAsset>(path_AppRootConfig);
        rootConfig = XmlSerializeManager.ProtoDeSerialize<RootScriptConfig>(config.bytes);
        for (int i = 0; i < rootConfig.RootScript.Count; i++)
        {
            IRoot iRoot;
            //判断是否存在XLua脚本，如果存在，执行XLua代码，不存在执行C#代码
            if (RunXLuaScripts && XLuaManager.Instance.IsLuaFileExist(rootConfig.RootScript[i].LuaScriptPath))
            {
                XLuaRoot root = new XLuaRoot();
                root.Init(rootConfig.RootScript[i].LuaScriptPath);
                iRoot = root as IRoot;
            }
            else
            {
                Type type = Type.GetType(rootConfig.RootScript[i].ScriptName);
                object obj = Activator.CreateInstance(type);
                iRoot = obj as IRoot;
            }
            if (!iRootPairs.ContainsKey(rootConfig.RootScript[i].ScriptName))
            {
                if (iRoot != null)
                {
                    iRootPairs.Add(rootConfig.RootScript[i].ScriptName, iRoot);
                }
                else
                {
                    Debuger.LogError("Root脚本为空 脚本名称:{0}", rootConfig.RootScript[i].ScriptName);
                }
            }
            if (!sceneScriptsPairs.ContainsKey(rootConfig.RootScript[i].SceneName))
            {
                List<string> scripts = new List<string>();
                scripts.Add(rootConfig.RootScript[i].ScriptName);
                sceneScriptsPairs.Add(rootConfig.RootScript[i].SceneName, scripts);
            }
            else
            {
                sceneScriptsPairs[rootConfig.RootScript[i].SceneName].Add(rootConfig.RootScript[i].ScriptName);
            }
        }
        callback?.Invoke();
    }
    public static void LoadScene(string sceneName, Action callback = null)
    {
        AssetsManager.Instance.LoadSceneAsync(sceneName, (AsyncOperation async) =>
        {
            if (async.isDone && async.progress == 1)
            {
                for (int i = 0; i < sceneScriptsPairs[sceneName].Count; i++)
                {
                    iRootPairs[sceneScriptsPairs[sceneName][i]].Begin();
                }
                callback?.Invoke();
            }
        });
    }

    public static void End()
    {
        HotfixRoot?.End();
        for (int i = 0; i < iRootPairs.Count; i++)
        {
            iRootPairs[rootConfig.RootScript[i].ScriptName].End();
        }
    }
}
