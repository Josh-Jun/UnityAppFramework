using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace App.Core.Helper
{
    public class AppHelper
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
    }
}