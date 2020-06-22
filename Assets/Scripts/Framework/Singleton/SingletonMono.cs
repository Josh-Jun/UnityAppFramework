using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_Instance;
    private static readonly string SingletonObject = "MonoObject";
    public static T Instance
    {
        get
        {
            lock (SingletonObject)
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType<T>();
                    if(s_Instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).ToString());
                        s_Instance = go.AddComponent<T>();
                    }
                }
            }
            return s_Instance;
        }
    }
}