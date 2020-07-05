using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace XLuaFrame {
    /// <summary> 窗口生命周期</summary>
    public enum WndLifeCycle {
        InitEvent,
        RegisterEvent,
        InitWindow,
        OpenWindow,
        CloseWindow,
        Update,
        OnDestroy,
    }

    /// <summary> 所有lua行为仅共享一个luaenv </summary>
    public class XLuaManager : SingletonMono<XLuaManager> {
        public static LuaEnv luaEnv = new LuaEnv(); //所有lua行为仅共享一个luaenv

        private static float lastGCTime = 0;//当前时间
        private const float GCInterval = 1;//每秒进行回收

        private const string LuaSuffixName = ".lua.txt";//Lua后缀名
        private string localPath;//本地路径

        private void Awake() {
            localPath = Application.dataPath.Replace("Assets", string.Empty);
        }

        private void Update() {
            if (Time.time - lastGCTime > GCInterval) {
                luaEnv.Tick();
                lastGCTime = Time.time;
            }
        }

        /// <summary> 加载Lua</summary>
        public byte[] LoadLua(ref string filePath) {
            string absPath = string.Format("{0}{1}{2}{3}", localPath, "XLuaScript/", filePath, LuaSuffixName);
            return System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(absPath));
        }

        /// <summary> 加载Lua</summary>
        public string LoadLua(string filePath) {
            string absPath = string.Format("{0}{1}{2}{3}", localPath, "XLuaScript/", filePath, LuaSuffixName);
            return File.ReadAllText(absPath);
        }

        /// <summary> 运行Lua </summary>
        public void RunLua(LuaEnv luaEnv, string luaFileName) {
            if (luaEnv != null) {
                luaEnv.DoString(ReturnLuaStr(luaFileName));//执行Lua文件
            } else {
                Debug.LogErrorFormat("LuaEnv为空,运行Lua:{0}{1}", luaFileName, LuaSuffixName);
            }
        }

        /// <summary> 释放Lua </summary>
        public void DisposeLua(LuaEnv luaEnv, string luaFileName) {
            if (luaEnv != null) {
                luaEnv.DoString(ReturnLuaStr(luaFileName));//执行Lua文件
            } else {
                Debug.LogErrorFormat("LuaEnv为空,释放Lua:{0}{1}", luaFileName, LuaSuffixName);
            }
        }

        /// <summary> 返回Lua字符串 </summary>
        public string ReturnLuaStr(string luaFileName) {
            return string.Format("{0}'{1}'", "require", luaFileName);
        }
    }
}