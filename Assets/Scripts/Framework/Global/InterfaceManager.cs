/// <summary>Root脚本接口</summary>
public interface IRoot
{
    void Begin();
    void End();
}
/// <summary>单例池接口</summary>
public interface ISingleton
{
    void Clear();
}
/// <summary>安卓包不同渠道</summary>
public enum ApkTarget
{
    Android = 0,
    PicoVR = 1
}