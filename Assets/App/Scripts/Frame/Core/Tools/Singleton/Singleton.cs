using App.Core.Helper;

namespace App.Core.Tools
{
    public class Singleton<T> : ISingleton where T : class, ISingleton, new()
    {
        private static T _Instance;
        private static readonly object SingletonLock = "Lock";

        public static T Instance
        {
            get
            {
                lock (SingletonLock)
                {
                    if (null == _Instance)
                    {
                        _Instance = new T();
                    }

                    return _Instance;
                }
            }
        }

        protected Singleton()
        {
            _Instance = this as T;
            SingletonManager.Instance.Add(_Instance);
        }

        public virtual void Clear()
        {
            _Instance = null;
        }
    }
}
