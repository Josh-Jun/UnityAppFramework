using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLuaFrame;

public class NewBehaviourScript : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        UIWindowBase window = this.TryGetComponect<XLuaUIWindow>().InitXlua("XLuaScript/Test/Test");
        window.SetWindowActive();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
