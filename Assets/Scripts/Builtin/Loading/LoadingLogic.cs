/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 进度条场景(Logic) - 加载场景过渡功能
 * ===============================================
 * */

using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;

namespace App.Modules.Loading
{
    [LogicOf(AssetPath.LoadingScene)]
    public class LoadingLogic : SingletonEvent<LoadingLogic>, ILogic
    {
        private LoadingView view;
        public LoadingLogic()
        {
            
        }
        public void Begin()
        {
            view = ViewMaster.Instance.GetView<LoadingView>();
            LoadScene();
        }

        private void LoadScene()
        {
            view.SetViewActive(true);
            SendEventMsg("BeforeLoadSceneEvent", SceneMaster.Instance.CurrentScene);
            var handle = Assets.LoadSceneAsync(SceneMaster.Instance.TargetScene, AssetPackage.HotfixPackage);
            var time_id = TimeUpdateMaster.Instance.StartTimer((time) =>
            {
                if (handle == null) return;
                view.SetLoadingSliderValue(handle.Progress);
            });
            handle.Completed += sceneHandle =>
            {
                TimeUpdateMaster.Instance.EndTimer(time_id);
                view.SetLoadingSliderValue(handle.Progress);
                SendEventMsg("AfterLoadSceneEvent", SceneMaster.Instance.TargetScene);
            };
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