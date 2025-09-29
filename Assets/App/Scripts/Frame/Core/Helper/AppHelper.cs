using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace App.Core.Helper
{
    public static class AppHelper
    {
        public static List<Type> GetAssemblyTypes<T>(string assemblyString = "App.Module")
        {
            if(Global.AppConfig.AssetPlayMode != YooAsset.EPlayMode.EditorSimulateMode)
            {
                var assembly = Global.AssemblyPairs[$"{assemblyString}"];
                var types = assembly.GetTypes();
                return types.Where(type => type.Name != typeof(T).Name && typeof(T).IsAssignableFrom(type)).ToList();
            }
            else
            {
                var assembly = Assembly.Load(assemblyString);
                var types = assembly.GetTypes();
                return types.Where(type => type.Name != typeof(T).Name && typeof(T).IsAssignableFrom(type)).ToList();
            }
        }

        public static Type GetAssemblyType<T>(string assemblyString = "App.Module")
        {
            if(Global.AppConfig.AssetPlayMode != YooAsset.EPlayMode.EditorSimulateMode)
            {
                return Global.AssemblyPairs[$"{assemblyString}"].GetType(typeof(T).FullName!);
            }   
            else
            {
                return Assembly.Load(assemblyString).GetType(typeof(T).FullName!);
            }
        }

        public static Type GetAssemblyType(string scriptName, string assemblyString = "App.Module")
        {
            if(Global.AppConfig.AssetPlayMode != YooAsset.EPlayMode.EditorSimulateMode)
            {
                return Global.AssemblyPairs[$"{assemblyString}"].GetType(scriptName);
            }   
            else
            {
                return Assembly.Load(assemblyString).GetType(scriptName);
            }
        }

        public static int GetIntData(string name)
        {
            return Global.CloudCtrl.IntData.Exists(i => i.name == name) ? Global.CloudCtrl.IntData.Find(i => i.name == name).value : 0;
        }

        public static float GetFloatData(string name)
        {
            return Global.CloudCtrl.FloatData.Exists(f => f.name == name) ? Global.CloudCtrl.FloatData.Find(f => f.name == name).value : 0;
        }

        public static string GetStringData(string name)
        {
            return Global.CloudCtrl.StringData.Exists(s => s.name == name) ? Global.CloudCtrl.StringData.Find(s => s.name == name).value : "";
        }

        public static bool GetBoolData(string name)
        {
            return !Global.CloudCtrl.BoolData.Exists(b => b.name == name) || Global.CloudCtrl.BoolData.Find(b => b.name == name).value;
        }
    }
}