using App.Core.Helper;

namespace App.Core.Tools
{
    public class Singleton<T> : ISingleton where T : class, ISingleton, new()
    {
        private static T _Instance;
        private const string SingletonLock = "Lock";

        public static T Instance
        {
            get
            {
                lock (SingletonLock)
                {
                    if (_Instance == null)
                    {
                        _Instance = new T();
                        SingletonManager.Instance.Add(_Instance);
                    }
                    return _Instance;
                }
            }
        }

        protected Singleton()
        {
            _Instance = this as T;
        }

        public virtual void Clear()
        {
            _Instance = null;
        }
    }
}