/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年8月8 15:27
 * function    :
 * ===============================================
 * */

using System.ComponentModel;
using App.Core.Master;

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