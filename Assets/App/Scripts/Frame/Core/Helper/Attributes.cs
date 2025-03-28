/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年10月19 13:40
 * function    :
 * ===============================================
 * */

using System;

namespace App.Core.Helper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LogicOfAttribute : System.Attribute
    {
        // Logic脚本所属的场景的路径
        public string Scene { get; private set; }

        public LogicOfAttribute(string Scene)
        {
            this.Scene = Scene;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ViewOfAttribute : System.Attribute
    {
        // View类型
        public ViewMold View { get; private set; }
        // 2D UI层级
        public int Layer { get; private set; }
        // 预制体全路径
        public string Location { get; private set; }
        // 默认显隐状态，默认关闭
        public bool Active { get; private set; }

        public ViewOfAttribute(ViewMold View, string Location, bool Active = false, int Layer = 0)
        {
            this.View = View;
            this.Layer = Layer;
            this.Active = Active;
            this.Location = Location;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigAttribute : System.Attribute { public ConfigAttribute() { } }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : System.Attribute
    {
        // 事件名称用来触发事件
        public string Event { get; private set; }

        public EventAttribute(string Event)
        {
            this.Event = Event;
        }
    }
}