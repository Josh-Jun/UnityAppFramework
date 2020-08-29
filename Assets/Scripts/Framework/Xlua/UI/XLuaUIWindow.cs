using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace XLuaFrame
{
    /// <summary> LuaUI类</summary>
    public class XLuaUIWindow : UIWindowBase {
        private readonly Dictionary<int, Action> LuaCallbackPairs = new Dictionary<int, Action>();//生命周期回调字典
        private LuaTable scriptEnv;
        /// <summary>初始化</summary>
        public XLuaUIWindow Init(string path)
        {
            string luaFileContent = XLuaManager.Instance.LoadLua(path);
            InitLuaEnv(luaFileContent);
            return this;
        }
        /// <summary>初始化</summary>
        protected override void InitWindow()
        {
            base.InitWindow();
            LuaCallbackPairs[(int)WindowLifeCycle.InitWindow]?.Invoke();
        }
        /// <summary>注册事件</summary>
        protected override void RegisterEvent() {
            base.RegisterEvent();
            LuaCallbackPairs[(int)WindowLifeCycle.RegisterEvent]?.Invoke();
        }
        /// <summary>打开窗口</summary>
        protected override void OpenWindow() {
            base.OpenWindow();
            LuaCallbackPairs[(int)WindowLifeCycle.OpenWindow]?.Invoke();
        }
        /// <summary>关闭窗口</summary>
        protected override void CloseWindow() {
            base.CloseWindow();
            LuaCallbackPairs[(int)WindowLifeCycle.CloseWindow]?.Invoke();
        }
        /// <summary>初始化Lua代码</summary>
        private void InitLuaEnv(string luaFileContent) {
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
            //这里的"this"和上面的"__index"是一个道理啦。将c#脚本绑定到LuaTable
            scriptEnv.Set("this", this);

            //执行lua语句，三个参数的意思分别是lua代码，lua代码在c#里的代号，lua代码在lua虚拟机里的代号
            XLuaManager.luaEnv.DoString(luaFileContent, this.name, scriptEnv);
            //xlua搞了这么久，也就是为了最后这几个锤子，c#调用lua里的方法。
            //总结起来一句话，通过luatable这个类来完成c#调用Lua
            //怎样完成这一步呢？就是获取luatable对象，配置lua虚拟机，配置虚拟机环境，绑定c#代码，最后执行lua语句
            foreach (int item in Enum.GetValues(typeof(WindowLifeCycle))) {
                string enumName = Enum.GetName(typeof(WindowLifeCycle), item);
                LuaCallbackPairs.Add(item, scriptEnv.Get<Action>(enumName));
            }
        }

        private void Update() {
            LuaCallbackPairs[(int)WindowLifeCycle.Update]?.Invoke();
        }

        private void OnDestroy() {
            LuaCallbackPairs[(int)WindowLifeCycle.OnDestroy]?.Invoke();
            LuaCallbackPairs.Clear();
            scriptEnv.Dispose();
        }
    }
}