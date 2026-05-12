/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月12 10:0
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using App.Core.Master;
using App.Core.Tools;
using UnityEngine;

namespace App.Modules
{
    public class AskBase : EventBaseMono
    {
        public virtual void Show(AskData data)
        {
        }
        
        protected virtual void Hide()
        {
            ViewMaster.Instance.CloseView<AskView>();
            Destroy(gameObject);
        }
    }
}
