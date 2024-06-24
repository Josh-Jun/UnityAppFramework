/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    :
 * ===============================================
 * */

using App;
using AppFrame.Info;
using AppFrame.Interface;
using AppFrame.Manager;
using AppFrame.Tools;
using UnityEngine.SceneManagement;

namespace Modules.Loading
{
    public class LoadingLogic : SingletonEvent<LoadingLogic>, ILogic
    {
        private LoadingView view;
        public LoadingLogic()
        {

        }
        public void Begin()
        {
            if (view == null)
            {
                //加载窗体
                view = AssetsManager.Instance.LoadUIView<LoadingView>(AppInfo.AssetPathPairs["LoadingView"]);
            }
        }

        public void LoadScene(string sceneName, bool isLoading = false, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            view.SetViewActive(isLoading);
            Root.LoadScene(sceneName, isLoading, progress =>
            {
                view.SetLoadingSliderValue(progress);
                if (progress >= 1)
                {
                    view.SetViewActive(false);
                }
            }, loadSceneMode);
        }
        public void End()
        {
            view.SetViewActive(false);
        }
        public void AppPause(bool pause)
        {
            
        }
        public void AppFocus(bool focus)
        {
            
        }
        public void AppQuit()
        {

        }
    }
}