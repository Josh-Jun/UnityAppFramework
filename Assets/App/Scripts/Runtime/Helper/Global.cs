using System.Collections.Generic;
using System.Reflection;
using App.Runtime.Helper;

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