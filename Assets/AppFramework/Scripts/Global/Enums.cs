/// <summary>安卓包不同渠道</summary>
public enum TargetPackage
{
    Mobile = 0,
    Pico = 1,
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
/// <summary>PicoVR手柄事件类型</summary>
public enum TrackedEventType
{
    MenuDownEvent = 0,      //菜单按钮按下
    MenuUpEvent = 1,        //菜单按钮抬起
    TriggerDownEvent = 2,   //扳机按钮按下
    TriggerUpEvent = 3,     //扳机按钮抬起
    GripDownEvent = 4,      //侧边按钮按下
    GripUpEvent = 5,        //侧边按钮抬起
    JoystickDownEvent = 6,  //摇杆按钮按下
    JoystickUpEvent = 7,    //摇杆按钮抬起
    XADownEvent = 8,        //XA按钮按下
    XAUpEvent = 9,          //XA按钮抬起
    YBDownEvent = 10,       //YB按钮按下
    YBUpEvent = 11,         //YB按钮抬起
}