/// <summary>Root脚本接口</summary>
public interface IRoot
{
    void Begin();
    void AppPause(bool pause);
    void AppFocus(bool focus);
    void End();
}
/// <summary>单例池接口</summary>
public interface ISingleton
{
    void Clear();
}