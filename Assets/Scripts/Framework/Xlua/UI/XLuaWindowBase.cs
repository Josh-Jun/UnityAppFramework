using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace XLuaFrame {
    /// <summary> LuaUI基类</summary>
    public class XLuaWindowBase : UIWindowBase {
        private Dictionary<int, Action> cbLuaDic = new Dictionary<int, Action>();//生命周期回调字典
        private Dictionary<int, Action<object>> cbLuaParDic = new Dictionary<int, Action<object>>();//生命周期回调字典
        private LuaTable scriptEnv;

        /// <summary>初始化</summary>
        public XLuaWindowBase InitXlua(string wndName) {
            InitLuaEnv(wndName);
            return this;
        }

        protected override void InitEvent()
        {
            base.InitEvent();
            cbLuaDic[(int)WndLifeCycle.InitEvent]?.Invoke();
        }

        protected override void RegisterEvent(bool isRemove) {
            base.RegisterEvent(isRemove);
            cbLuaParDic[(int)WndLifeCycle.RegisterEvent]?.Invoke(isRemove);
        }

        protected override void OpenWindow() {
            base.OpenWindow();
            cbLuaDic[(int)WndLifeCycle.OpenWindow]?.Invoke();
        }
        protected override void CloseWindow() {
            base.CloseWindow();
            cbLuaDic[(int)WndLifeCycle.CloseWindow]?.Invoke();
        }
        protected override void InitWindow()
        {
            base.InitWindow();
            cbLuaDic[(int)WndLifeCycle.InitWindow]?.Invoke();
        }


        private void InitLuaEnv(string luaFileName) {
            //获取LuaTable，要用luaEnv.NewTable()，这里会做一些排序，共用虚拟机等操作
            scriptEnv = XLuaManager.luaEnv.NewTable();
            LuaTable meta = XLuaManager.luaEnv.NewTable();
            //key,value的方式，虚拟机环境对应的key值一定要是这个“__index”，
            //在xlua的底层，获取LuaTable所属虚拟机的环境是get的时候，用的key是这个名字，所以不能改
            meta.Set("__index", XLuaManager.luaEnv.Global);
            //将有虚拟机和全局环境的table绑定成他自己的metatable
            scriptEnv.SetMetaTable(meta);
            //值已经传递过去了，就释放他
            meta.Dispose();
            //这里的"self"和上面的"__index"是一个道理啦。将c#脚本绑定到LuaTable
            scriptEnv.Set("this", this);

            //执行lua语句，三个参数的意思分别是lua代码，lua代码在c#里的代号，lua代码在lua虚拟机里的代号
            XLuaManager.luaEnv.DoString(XLuaManager.Instance.LoadLua(luaFileName), this.name, scriptEnv);
            //xlua搞了这么久，也就是为了最后这几个锤子，c#调用lua里的方法。
            //总结起来一句话，通过luatable这个类来完成c#调用Lua
            //怎样完成这一步呢？就是获取luatable对象，配置lua虚拟机，配置虚拟机环境，绑定c#代码，最后执行lua语句
            foreach (int item in Enum.GetValues(typeof(WndLifeCycle))) {
                string enumName = Enum.GetName(typeof(WndLifeCycle), item);
                switch (enumName) {
                    case "Event":
                        cbLuaParDic.Add(item, scriptEnv.Get<Action<object>>(enumName));
                        break;
                    default:
                        cbLuaDic.Add(item, scriptEnv.Get<Action>(enumName));
                        break;
                }
            }
        }

        private void Update() {
            cbLuaDic[(int)WndLifeCycle.Update]?.Invoke();
        }

        private void OnDestroy() {
            cbLuaDic[(int)WndLifeCycle.OnDestroy]?.Invoke();
            cbLuaDic.Clear();
            scriptEnv.Dispose();
        }
    }
}