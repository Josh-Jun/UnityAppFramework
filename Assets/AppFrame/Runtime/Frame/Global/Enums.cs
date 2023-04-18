
namespace AppFrame.Enum
{
    /// <summary>更新类型</summary>
    public enum UpdateMold
    {
        None = 0,//不更新
        Hotfix = 1,//更新资源
        App = 2,//更新安装包
    }

    /// <summary>配置表类型</summary>
    public enum TableMold
    {
        Json = 0,
        Xml = 1,
    }

    /// <summary>AB包类型</summary>
    public enum ABMold
    {
        Hybrid = 0,//Dll资源AB
        Assets = 1,//资源AB
    }

    /// <summary>AB包类型</summary>
    public enum ViewMold
    {
        UI2DView = 0,//2DUI
        UI3DView = 1,//3DUI
        GOView = 1,//3DModel
    }
}