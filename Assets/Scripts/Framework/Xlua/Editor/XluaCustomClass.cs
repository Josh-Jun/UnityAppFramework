using System.Collections.Generic;
using System.Linq;
namespace XLua {
    /// <summary> Xlua拓展类 </summary>
    public static class XluaCustomClass {
        /// <summary> 批量打[Hotfix]标签 </summary>
        [XLua.Hotfix]
        [XLua.ReflectionUse]
        public static List<System.Type> Developer {
            //1、扩展类、2、Xlua框架
            get {
                return (from type in System.Reflection.Assembly.Load("Assembly-CSharp").GetTypes()
                        where type.Name == "DevelopManager" ||
                              type.Namespace == "XLuaFrame"
                        select type).ToList();
            }
        }

        /// <summary> 批量打[LuaCallCSharp]标签,lua中要使用到C#库的配置 </summary>
        [XLua.LuaCallCSharp]
        [XLua.ReflectionUse]
        public static List<System.Type> LuaCallCSharp {
            //1、扩展类、2、消息事件控制中心、3事件监听者
            get {
                return (from type in System.Reflection.Assembly.Load("Assembly-CSharp").GetTypes()
                        where type.Name == "DevelopManager" ||
                              type.Namespace == "EventController" ||
                              type.Namespace == "EventListener"
                        select type).ToList();
            }
        }

        /// <summary> 批量打[CSharpCallLua]标签,C#静态调用Lua的配置 </summary>
        [XLua.CSharpCallLua]
        [XLua.ReflectionUse]
        public static List<System.Type> CSharpCallLua = new List<System.Type>(){
            typeof(System.Action<System.Object>),
            typeof(System.Action<System.Object[]>),
            typeof(System.Action<bool,System.Object>),
            typeof(System.Action<bool,System.Object[]>),
            typeof(System.Action<float,System.Object>),
            typeof(System.Action<float,System.Object[]>),
            typeof(System.Action<int,System.Object>),
            typeof(System.Action<int,System.Object[]>),
            typeof(System.Action<string,System.Object>),
            typeof(System.Action<string,System.Object[]>),
            typeof(System.Action<UnityEngine.Vector2,System.Object>),
            typeof(System.Action<UnityEngine.Vector2,System.Object[]>),
        };
    }
}
