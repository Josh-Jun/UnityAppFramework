/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年6月24 13:57
 * function    : 弹窗功能(Logic) - 1,确认弹窗  2,提示弹窗
 * ===============================================
 * */
using System;
using App.Core.Helper;
using App.Core.Master;
using App.Core.Tools;

namespace App.Modules
{
    [LogicOf(AssetPath.Global)]
    public class AskLogic : SingletonEvent<AskLogic>, ILogic
    {
        private AskView View => ViewMaster.Instance.GetView<AskView>();
        public AskLogic()
        {
            AddEventMsg<string, Action, Action, string, string>("ShowAskViewAsBtnText", ShowAskView);
            AddEventMsg<string, Action, Action>("ShowAskView", ShowAskView);
            AddEventMsg<string, float>("ShowTips", ShowTips);
        }
        public void Begin()
        {
            View.OpenView();
        }
        public void End()
        {
            View.CloseView();
        }
        public void ShowAskView(string content, Action confirm_callback, Action cancel_callback, string confirm, string cancel)
        {
            View.SetViewInfo(content, confirm_callback, cancel_callback, confirm, cancel);
        }
        public void ShowAskView(string content, Action confirm_callback, Action cancel_callback)
        {
            View.SetViewInfo(content, confirm_callback, cancel_callback);
        }

        public void ShowTips(string content, float time = 2f)
        {
            View.SetTips(content, time);
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