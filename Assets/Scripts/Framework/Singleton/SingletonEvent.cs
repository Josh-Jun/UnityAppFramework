public class SingletonEvent<T> : EventBase where T : EventBase, new()
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
}
