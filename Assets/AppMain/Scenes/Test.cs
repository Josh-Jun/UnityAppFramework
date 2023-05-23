using System.Collections;
using System.Collections.Generic;
using AppFrame.Enum;
using AppFrame.Tools;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        TimeUpdateManager.Instance.StartTimer((time) =>
        {
            Log.I(time);
        }, UpdateMold.FixedUpdate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
