/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年10月21 16:4
 * function    : 
 * ===============================================
 * */
using System;
using System.Collections;
using System.Collections.Generic;
using AppFrame.Interface;
using AppFrame.Tools;
using AppFrame.Manager;
using AppFrame.Attribute;
using AppFrame.View;
using UnityEngine;

namespace Modules.NewScript
{
    [LogicOf(Assets.Global)]
    public class NewScriptLogic : SingletonEvent<NewScriptLogic>, ILogic
    {
        private NewScriptView view;
        
        public NewScriptLogic()
        {
            
        }
        public void Begin()
        {
            view = ViewManager.Instance.GetView<NewScriptView>();
        }
        public void End()
        {
            
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