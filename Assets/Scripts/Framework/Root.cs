using Hotfix;
using System;
using System.Collections.Generic;
using UnityEngine;
using XLuaFrame;

public class Root
{
    private const string path_HotfixScene = "A0_App/Scenes/HotfixScene";
    private const string path_AppRootConfig = "AppRootConfig";
    private static RootScriptConfig rootConfig;
    private static Dictionary<string, IRoot> iRootPairs = new Dictionary<string, IRoot>();
    public static void Init()
    {
        AssetsManager.Instance.LoadSceneAsync(path_HotfixScene, (ao) =>
        {
            if (ao.isDone)
            {
                TextAsset config = Resources.Load<TextAsset>(path_AppRootConfig);
                rootConfig = XmlSerializeManager.ProtoDeSerialize<RootScriptConfig>(config.bytes);
                for (int i = 0; i < rootConfig.RootScript.Count; i++)
                {
                    IRoot iRoot = null;
                    if (XLuaManager.Instance.IsLuaFileExist(rootConfig.RootScript[i].LuaScriptPath))
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
                            Debug.LogErrorFormat("Root脚本为空 脚本名称:{0}", rootConfig.RootScript[i].ScriptName);
                        }
                    }
                }
            }
        });
    }
    public static void Begin()
    {
        for (int i = 0; i < iRootPairs.Count; i++)
        {
            iRootPairs[rootConfig.RootScript[i].ScriptName].Begin();
        }
    }
    public static void End()
    {
        for (int i = 0; i < iRootPairs.Count; i++)
        {
            iRootPairs[rootConfig.RootScript[i].ScriptName].End();
        }
    }
}
