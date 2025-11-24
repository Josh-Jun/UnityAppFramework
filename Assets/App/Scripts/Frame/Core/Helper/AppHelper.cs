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
            var assembly = Global.AppConfig.AssetPlayMode != YooAsset.EPlayMode.EditorSimulateMode ? Global.AssemblyPairs[$"{assemblyString}"] : Assembly.Load(assemblyString);
            var types = assembly.GetTypes();
            return types.Where(type => type.Name != typeof(T).Name && typeof(T).IsAssignableFrom(type)).ToList();
        }

        public static Type GetAssemblyType<T>(string assemblyString = "App.Module")
        {
            return Global.AppConfig.AssetPlayMode != YooAsset.EPlayMode.EditorSimulateMode ? Global.AssemblyPairs[$"{assemblyString}"].GetType(typeof(T).FullName!) : Assembly.Load(assemblyString).GetType(typeof(T).FullName!);
        }

        public static Type GetAssemblyType(string scriptName, string assemblyString = "App.Module")
        {
            return Global.AppConfig.AssetPlayMode != YooAsset.EPlayMode.EditorSimulateMode ? Global.AssemblyPairs[$"{assemblyString}"].GetType(scriptName) : Assembly.Load(assemblyString).GetType(scriptName);
        }

        public static T GetData<T>(string key)
        {
            object value = null;
            if (typeof(T) == typeof(int))
            {
                value = Global.CloudCtrl.IntData.Exists(i => i.name == key) ? Global.CloudCtrl.IntData.Find(i => i.name == key).value : 0;
            }
            if (typeof(T) == typeof(float))
            {
                value = Global.CloudCtrl.FloatData.Exists(f => f.name == key) ? Global.CloudCtrl.FloatData.Find(f => f.name == key).value : 0;
            }
            if (typeof(T) == typeof(string))
            {
                value = Global.CloudCtrl.StringData.Exists(s => s.name == key) ? Global.CloudCtrl.StringData.Find(s => s.name == key).value : "";
            }
            if (typeof(T) == typeof(bool))
            {
                value = !Global.CloudCtrl.BoolData.Exists(b => b.name == key) || Global.CloudCtrl.BoolData.Find(b => b.name == key).value;
            }
            return (T)value;
        }
    }
}