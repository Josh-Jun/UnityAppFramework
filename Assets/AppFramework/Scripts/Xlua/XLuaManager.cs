using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

namespace XLuaFrame
{
    /// <summary> 窗口生命周期</summary>
    public enum WindowLifeCycle
    {
        InitWindow,
        RegisterEvent,
        OpenWindow,
        CloseWindow,
        Update,
        OnDestroy,
    }

    /// <summary> Root生命周期</summary>
    public enum RootLifeCycle
    {
        Init,
        Begin,
        End,
        AppQuit,
    }
    /// <summary> Root生命周期</summary>
    public enum RootLifeCycleBool
    {
        AppPause,
        AppFocus,
    }
    /// <summary> 所有lua行为仅共享一个luaenv </summary>
    public class XLuaManager : SingletonMono<XLuaManager>
    {
        public static LuaEnv luaEnv = new LuaEnv(); //所有lua行为仅共享一个luaenv

        private static float lastGCTime = 0;//当前时间
        private const float GCInterval = 1;//每秒进行回收

        private const string LuaSuffixName = ".lua";//Lua后缀名

        private void Update()
        {
            if (Time.time - lastGCTime > GCInterval)
            {
                luaEnv.Tick();
                lastGCTime = Time.time;
            }
        }

        /// <summary> 加载Lua</summary>
        public byte[] LoadLua(ref string filePath)
        {
            string path = string.Format("XLuaScripts/{0}{1}", filePath, LuaSuffixName);
            TextAsset textAsset = AssetsManager.Instance.LoadAsset<TextAsset>(path);
            return System.Text.Encoding.UTF8.GetBytes(textAsset.text);
        }

        /// <summary> Lua文件是否存在 </summary>
        public bool IsLuaFileExist(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string path = string.Format("XLuaScripts/{0}{1}", filePath, LuaSuffixName);
                TextAsset textAsset = AssetsManager.Instance.LoadAsset<TextAsset>(path);
                return !string.IsNullOrEmpty(textAsset.text);
            }
            return false;
        }


        /// <summary> 加载Lua</summary>
        public string LoadLua(string filePath)
        {
            string path = string.Format("XLuaScripts/{0}{1}", filePath, LuaSuffixName);
            TextAsset textAsset = AssetsManager.Instance.LoadAsset<TextAsset>(path);
            return textAsset.text;
        }

        /// <summary> 运行Lua </summary>
        public void RunLua(LuaEnv luaEnv, string luaFileName)
        {
            if (luaEnv != null)
            {
                luaEnv.DoString(ReturnLuaStr(luaFileName));//执行Lua文件
            }
            else
            {
                Debug.LogError($"LuaEnv为空,运行Lua:{luaFileName}{LuaSuffixName}");
            }
        }

        /// <summary> 释放Lua </summary>
        public void DisposeLua(LuaEnv luaEnv, string luaFileName)
        {
            if (luaEnv != null)
            {
                luaEnv.DoString(ReturnLuaStr(luaFileName));//执行Lua文件
            }
            else
            {
                Debug.LogError($"LuaEnv为空,释放Lua:{luaFileName}{LuaSuffixName}");
            }
        }

        /// <summary> 返回Lua字符串 </summary>
        public string ReturnLuaStr(string luaFileName)
        {
            return string.Format("{0}'{1}'", "require", luaFileName);
        }
    }
}