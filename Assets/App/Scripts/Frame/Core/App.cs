using UnityEngine;

namespace App.Core
{
    /// <summary>
    /// 程序入口
    /// </summary>
    public class App : MonoBehaviour
    {
        private static App app = null;

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

        /// <summary>
        /// 移动端：当玩家home退出到桌面的时候发送给所有游戏对象.
        /// </summary>
        /// <param name="pause">是否暂停</param>
        private void OnApplicationPause(bool pause)
        {
            Root.ExecuteMethods("AppPause", pause);
        }

        /// <summary>
        /// 当玩家获取或失去焦点的时候发送给所有游戏对象.
        /// </summary>
        /// <param name="focus">是否暂停</param>
        private void OnApplicationFocus(bool focus)
        {
            Root.ExecuteMethods("AppFocus", focus);
        }

        private void OnApplicationQuit()
        {
            //程序退出接口实现
            Root.ExecuteMethods("AppQuit");
        }
    }
}