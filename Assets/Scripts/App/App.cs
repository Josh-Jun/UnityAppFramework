using UnityEngine;
/// <summary>
/// 程序入口
/// </summary>
public class App : MonoBehaviour
{
    public static App app = null;
    public const bool IsHotfix = false;
    private void Awake()
    {
        if (app == null)
        {
            //单例初始化
            app = this;
            //切换场景不销毁此对象
            DontDestroyOnLoad(gameObject);
            //禁止程序休眠
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //设置程序帧率
            Application.targetFrameRate = 60;
            //启动入口
            Root.Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //开始接口实现
        Root.Begin();
    }
    private void OnDestroy()
    {
        //结束接口实现
        Root.End();
    }
}
