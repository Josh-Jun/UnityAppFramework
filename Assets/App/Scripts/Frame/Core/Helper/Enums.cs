namespace App.Core.Helper
{
    /// <summary>配置表类型</summary>
    public enum ConfigMold
    {
        Json = 0,
        Xml = 1,
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