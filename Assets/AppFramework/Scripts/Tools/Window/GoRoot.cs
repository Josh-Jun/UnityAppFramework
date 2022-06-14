using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoRoot : SingletonMono<GoRoot>
{
    private Dictionary<string, Transform> rootPairs = new Dictionary<string, Transform>();

    /// <summary> 获取3D游戏对象根对象 </summary>
    public Transform GoTransform { get { return transform; } private set { } }
    // Start is called before the first frame update
    void Awake()
    {

    }

    /// <summary> 添加3D对象预制体，返回GameObject</summary>
    public GameObject AddChild(GameObject prefab, Transform parent = null)
    {
        Transform objParent = parent ? parent : GoTransform;
        GameObject go = Instantiate(prefab, objParent);
        return go;
    }

    /// <summary> 添加3D对象预制体，返回GameObject </summary>
    public Transform TryGetEmptyNode(string name)
    {
        if (!rootPairs.ContainsKey(name)) {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            rootPairs.Add(name, go.transform);
            return go.transform;
        } else {
            return rootPairs[name];
        }
    }

}
