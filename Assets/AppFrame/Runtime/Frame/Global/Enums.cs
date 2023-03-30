
namespace AppFrame.Enum
{
    /// <summary>安卓包不同渠道</summary>
    public enum TargetPackage
    {
        Mobile = 0,
        XR = 1,
    }

    /// <summary>AB包打包加载方式</summary>
    public enum ABPipeline
    {
        Default = 0,
        Scriptable = 1,
    }

    /// <summary>更新类型</summary>
    public enum UpdateMold
    {
        None = 0,
        Hotfix = 1,
        App = 2,
    }

    /// <summary>配置表类型</summary>
    public enum TableMold
    {
        Json = 0,
        Xml = 1,
    }

    /// <summary>资源加载类型</summary>
    public enum LoadAssetsMold
    {
        Native = 0,
        Local = 1,
        Remote = 2,
    }
    
    /// <summary>AB包类型</summary>
    public enum ABMold
    {
        Hybrid = 0,
        Assets = 1,
    }
}