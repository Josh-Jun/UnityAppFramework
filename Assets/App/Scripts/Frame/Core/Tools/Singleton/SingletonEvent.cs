using App.Core.Helper;

namespace App.Core.Tools
{
    public class SingletonEvent<T> : EventBase, ISingleton where T : EventBase, ISingleton, new()
    {
        private static T _Instance;
        private static readonly object SingletonLock = "EventLock";

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

        protected SingletonEvent()
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
