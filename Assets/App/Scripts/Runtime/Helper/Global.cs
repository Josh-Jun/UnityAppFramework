using System.Collections.Generic;
using System.Reflection;
using App.Runtime.CloudCtrl;
using App.Runtime.Helper;
using UnityEngine;

public class Global
{
    public static AppConfig AppConfig;

    public static string HttpServer => DevelopmentEnvironments[AppConfig.DevelopmentMold].HttpServer;
    public static string SocketServer => DevelopmentEnvironments[AppConfig.DevelopmentMold].SocketServer;
    public static string CdnServer => DevelopmentEnvironments[AppConfig.DevelopmentMold].CdnServer;
    public static int SocketPort => DevelopmentEnvironments[AppConfig.DevelopmentMold].SocketPort;
    
    public static List<string> AOTMetaAssemblyNames { get; } = new () { "mscorlib.dll", "System.dll", "System.Core.dll", "UnityEngine.CoreModule.dll", };

    public static List<string> HotfixAssemblyNames { get; } = new () { "App.Core", "App.Module", };

    public static Dictionary<string, Assembly> AssemblyPairs { get; } = new();
    
    public static CloudCtrl CloudCtrl = new();
    
    public static string PlatformName
    {
        get
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget switch
            {
                UnityEditor.BuildTarget.iOS or
                    UnityEditor.BuildTarget.WebGL or
                    UnityEditor.BuildTarget.Android or
                    UnityEditor.BuildTarget.VisionOS => $"{UnityEditor.EditorUserBuildSettings.activeBuildTarget}",
                UnityEditor.BuildTarget.StandaloneWindows or
                    UnityEditor.BuildTarget.StandaloneWindows64 => $"Windows",
                UnityEditor.BuildTarget.StandaloneOSX => $"MacOS",
                _ => string.Empty
            };
#else
            return Application.platform switch
            {
                RuntimePlatform.VisionOS or
                    RuntimePlatform.Android => $"{Application.platform}",
                RuntimePlatform.IPhonePlayer => $"iOS",
                RuntimePlatform.OSXPlayer => $"MacOS",
                RuntimePlatform.WindowsPlayer => $"Windows",
                RuntimePlatform.WebGLPlayer => $"WebGL",
                _ => string.Empty
            };
#endif
        }
    }

    private static readonly Dictionary<DevelopmentMold, DevelopmentEnvironment> DevelopmentEnvironments = new ()
    {
        {
            DevelopmentMold.Test,
            new DevelopmentEnvironment()
            {
                HttpServer = "",
                SocketServer = "",
                CdnServer = "",
                SocketPort = 8080,
            }
        },
        {
            DevelopmentMold.Sandbox,
            new DevelopmentEnvironment()
            {
                HttpServer = "",
                SocketServer = "",
                CdnServer = "",
                SocketPort = 8080,
            }
        },
        {
            DevelopmentMold.Release,
            new DevelopmentEnvironment()
            {
                HttpServer = "",
                SocketServer = "",
                CdnServer = "",
                SocketPort = 8080,
            }
        },
        {
            DevelopmentMold.Local,
            new DevelopmentEnvironment()
            {
                HttpServer = "",
                SocketServer = "",
                CdnServer = "",
                SocketPort = 8080,
            }
        },
    };
}

public class DevelopmentEnvironment
{
    public string HttpServer;
    public string SocketServer;
    public string CdnServer;
    public int SocketPort;
}