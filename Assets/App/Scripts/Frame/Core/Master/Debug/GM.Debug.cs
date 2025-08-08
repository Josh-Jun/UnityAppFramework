/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月8 15:27
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using App.Core.Master;
using App.Core.Tools;
using SRDebugger;
using UnityEngine;

public partial class GM
{
    [Category("Args"), DisplayName("View Name")]
    public string ViewName { get; set; }
    
    [Category("CMD"), DisplayName("Open View")]
    public void OpenView()
    {
        ViewMaster.Instance.OpenView($"App.Modules.{ViewName}View");
    }
    
    [Category("CMD"), DisplayName("Close View")]
    public void CloseView()
    {
        ViewMaster.Instance.CloseView($"App.Modules.{ViewName}View");
    }
}