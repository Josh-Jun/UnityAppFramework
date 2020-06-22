using System;
using System.Collections.Generic;
using UnityEngine;

public class Root
{
    private const string path_HotfixScene = "A0_App/Scenes/HotfixScene";
    private const string path_AppRootConfig = "AppRootConfig";
    private static RootConfig rootConfig;
    private static Dictionary<string, IRoot> iRootPairs = new Dictionary<string, IRoot>();
    public static void Init()
    {
        AssetsManager.Instance.LoadSceneAsync(path_HotfixScene, (ao) =>
        {
            if (ao.isDone)
            {
                TextAsset config = AssetsManager.Instance.LoadLocalAsset<TextAsset>(path_AppRootConfig);
                rootConfig = XmlSerializeManager.ProtoDeSerialize<RootConfig>(config.bytes);
                for (int i = 0; i < rootConfig.ScriptNames.Count; i++)
                {
                    Type type = Type.GetType(rootConfig.ScriptNames[i]);
                    object obj = Activator.CreateInstance(type);
                    IRoot iRoot = obj as IRoot;
                    if (!iRootPairs.ContainsKey(rootConfig.ScriptNames[i]))
                    {
                        iRootPairs.Add(rootConfig.ScriptNames[i], iRoot);
                    }
                }
            }
        });
    }
    public static void Begin()
    {
        for (int i = 0; i < iRootPairs.Count; i++)
        {
            iRootPairs[rootConfig.ScriptNames[i]].Begin();
        }
    }
    public static void Finish()
    {
        for (int i = 0; i < iRootPairs.Count; i++)
        {
            iRootPairs[rootConfig.ScriptNames[i]].Finish();
        }
    }
}
