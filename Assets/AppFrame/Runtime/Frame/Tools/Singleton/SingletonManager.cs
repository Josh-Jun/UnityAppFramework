using System.Collections.Generic;
using AppFrame.Interface;

namespace AppFrame.Tools
{
    public class SingletonManager : Singleton<SingletonManager>
    {
        private readonly Dictionary<string, ISingleton> SingletonPool = new Dictionary<string, ISingleton>(); //单例池字典

        /// <summary> 添加单例 </summary>
        public void Add(ISingleton obj)
        {
            string name = obj.ToString();
            if (!SingletonPool.ContainsKey(name))
            {
                SingletonPool.Add(name, obj);
            }
            else
            {
                Log.I($"已具有相同名称的实例:{name}");
            }
        }

        /// <summary> 删除所有单例 </summary>
        public void DeleteAll()
        {
            foreach (var item in SingletonPool.Keys)
            {
                SingletonPool[item].Clear();
            }

            SingletonPool.Clear();
        }

        /// <summary> 删除指定单例 </summary>
        public void Delete(ISingleton obj)
        {
            string name = obj.ToString();
            if (SingletonPool.ContainsKey(name))
            {
                SingletonPool[name].Clear();
                SingletonPool.Remove(name);
            }
            else
            {
                Log.E($"实例不存在:{name}");
            }
        }

        /// <summary> 删除指定单例 </summary>
        public void Delete(string name)
        {
            if (SingletonPool.ContainsKey(name))
            {
                SingletonPool[name].Clear();
                SingletonPool.Remove(name);
            }
            else
            {
                Log.E($"实例不存在:{name}");
            }
        }
    }
}
