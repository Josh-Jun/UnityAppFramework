/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年9月15 11:40
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using App.Core.Tools;
using UnityEngine;

namespace App.Core.Master
{
    public class XRMaster : SingletonMonoEvent<XRMaster>
    {
        private Transform XRRoot;
        private void Awake()
        {
            XRRoot = this.FindComponent<Transform>("XR Root");
        }
    }
}
