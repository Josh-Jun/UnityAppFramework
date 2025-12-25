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
        public string Name { get; private set; }
        // Logic脚本所属的场景的路径
        public string Scene { get; private set; }

        public LogicOfAttribute(string Name, string Scene)
        {
            this.Scene = Scene;
            this.Name = Name;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ViewOfAttribute : System.Attribute
    {
        public string Name { get; private set; }
        // View类型
        public ViewMold View { get; private set; }
        // 2D UI层级
        public int Layer { get; private set; }
        // 预制体全路径
        public string Location { get; private set; }
        // 默认显隐状态，默认关闭
        public bool Active { get; private set; }

        public ViewOfAttribute(string Name, ViewMold View, string Location, bool Active = false, int Layer = 0)
        {
            this.Name = Name;
            this.View = View;
            this.Layer = Layer;
            this.Active = Active;
            this.Location = Location;
        }
#if UNITY_EDITOR
        public void SetLocation(string location)
        {
            this.Location = location;
        }
        
        public void SetName(string name)
        {
            this.Name = name;
        }

        public void SetActive(bool active)
        {
            this.Active = active;
        }

        public void SetLayer(int layer)
        {
            this.Layer = layer;
        }

        public void SetViewMold(ViewMold view)
        {
            this.View = view;
        }
#endif
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