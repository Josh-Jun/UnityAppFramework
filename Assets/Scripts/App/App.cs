using UnityEngine;
/// <summary>
/// 程序入口
/// </summary>
public class App : MonoBehaviour
{
    public static App app = null;
    private void Awake()
    {
        if (app == null)
        {
            //单例初始化
            app = this;
            //切换场景不销毁此对象
            DontDestroyOnLoad(gameObject);
            //启动入口
            Root.Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        //结束接口实现
        Root.End();
    }
}
