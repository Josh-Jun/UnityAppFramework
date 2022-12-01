
using System.Reflection;
using AppFramework.Enum;

namespace AppFramework.Info
{
    public class AppInfo
    {
        public static Assembly Assembly;
        public static ABPipeline AbPipeline;
        public static bool IsTestServer;
        public static bool IsHotfix;
        public static TargetPackage TargetPackage;
    }
}