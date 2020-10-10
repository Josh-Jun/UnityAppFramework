using Hotfix;
using System;
using System.Collections.Generic;
using UnityEngine;
using XLuaFrame;

public class Root
{
    private const string hotfixScriptName = "Hotfix.HotfixRoot";
    private const string path_AppRootConfig = "App/Assets/AppRootConfig";
    private static RootScriptConfig rootConfig;
    private static readonly Dictionary<string, IRoot> iRootPairs = new Dictionary<string, IRoot>();
    private static IRoot HotfixRoot = null;
    public static void Init()
    {
        Type type = Type.GetType(hotfixScriptName);
        object obj = Activator.CreateInstance(type);
        HotfixRoot = obj as IRoot;
    }

    public static void InitRootScripts()
    {
        TextAsset config = AssetsManager.Instance.LoadAsset<TextAsset>(path_AppRootConfig);
        rootConfig = XmlSerializeManager.ProtoDeSerialize<RootScriptConfig>(config.bytes);
        for (int i = 0; i < rootConfig.RootScript.Count; i++)
        {
            IRoot iRoot;
            //判断是否存在XLua脚本，如果存在，执行XLua代码，不存在执行C#代码
            if (XLuaManager.Instance.IsLuaFileExist(rootConfig.RootScript[i].LuaScriptPath) && App.RunXLuaScripts)
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

    public static void End()
    {
        HotfixRoot.End();
        for (int i = 0; i < iRootPairs.Count; i++)
        {
            iRootPairs[rootConfig.RootScript[i].ScriptName].End();
        }
    }
}
