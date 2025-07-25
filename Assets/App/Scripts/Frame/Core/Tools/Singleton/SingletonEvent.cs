using App.Core.Helper;

namespace App.Core.Tools
{
    public class SingletonEvent<T> : EventBase, ISingleton where T : EventBase, ISingleton, new()
    {
        private static T _Instance;
        private const string SingletonLock = "EventLock";

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

        protected SingletonEvent()
        {
            _Instance = this as T;
        }

        public virtual void Clear()
        {
            _Instance = null;
        }
    }
}