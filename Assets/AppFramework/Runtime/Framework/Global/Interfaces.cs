
namespace AppFramework.Interface
{
    /// <summary>Root脚本接口</summary>
    public interface ILogic
    {
        void Begin();
        void End();
        void AppPause(bool pause);
        void AppFocus(bool focus);
        void AppQuit();
    }

    /// <summary>单例池接口</summary>
    public interface ISingleton
    {
        void Clear();
    }
}