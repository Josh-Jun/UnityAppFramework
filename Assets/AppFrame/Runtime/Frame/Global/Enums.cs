
namespace AppFrame.Enum
{
    /// <summary>更新类型</summary>
    public enum HotfixMold
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
    
    /// <summary>View类型</summary>
    public enum ViewMold
    {
        UI2D = 0,//2DUI
        UI3D = 1,//3DUI
        Go3D = 2,//GameObject
    }

    /// <summary>更新方法类型</summary>
    public enum UpdateMold
    {
        Update = 0,//2DUI
        FixedUpdate = 1,//3DUI
        LateUpdate = 2,//3DModel
    }
}