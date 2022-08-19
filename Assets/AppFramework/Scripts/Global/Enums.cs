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
/// <summary>配置表类型</summary>
public enum TableMold
{
    Scriptable = 0,
    Json = 1,
    Xml = 2,
}
/// <summary>PicoVR手柄事件类型</summary>
public enum TrackedEventType
{
    LeftMenuDownEvent = 0,
    LeftMenuUpEvent = 1,
    LeftMenuingEvent = 2,
    RightMenuDownEvent = 3,
    RightMenuUpEvent = 4,
    RightMenuingEvent = 5,
    LeftTriggerDownEvent = 6,
    LeftTriggerUpEvent = 7,
    LeftTriggeringEvent = 8,
    RightTriggerDownEvent = 9,
    RightTriggerUpEvent = 10,
    RightTriggeringEvent = 11,
    LeftGripDownEvent = 12,
    LeftGripUpEvent = 13,
    LeftGripingEvent = 14,
    RightGripDownEvent = 15,
    RightGripUpEvent = 16,
    RightGripingEvent = 17,
    LeftJoystickDownEvent = 18,
    LeftJoystickUpEvent = 19,
    LeftJoystickingEvent = 20,
    RightJoystickDownEvent = 21,
    RightJoystickUpEvent = 22,
    RightJoystickingEvent = 23,
    LeftXADownEvent = 24,
    LeftXAUpEvent = 25,
    LeftXAingEvent = 26,
    RightXADownEvent = 27,
    RightXAUpEvent = 28,
    RightXAingEvent = 29,
    LeftYBDownEvent = 30,
    LeftYBUpEvent = 31,
    LeftYBingEvent = 32,
    RightYBDownEvent = 33,
    RightYBUpEvent = 34,
    RightYBingEvent = 35,
}