using System.Collections.Generic;
using AppFrame.Config;
using UnityEngine;

public class Global
{
    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "mscorlib.dll",
        "System.dll",
        "System.Core.dll",
        "UnityEngine.CoreModule.dll",
    };

    public static List<string> HotfixAssemblyNames { get; } = new List<string>()
    {
        "App.Frame.dll",
        "App.Module.dll",
    };

    public static AppConfig AppConfig;
    public static GameObject UpdateView;
}
