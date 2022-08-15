using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class TextHandler : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public ImeManager mManager;
    public SGImeInputType mInputType;
    public SGImeTextType mTextType;
    public Text Debugtext;
    // Start is called before the first frame update
    void Awake()
    {

    }
    void Update()
    {
      
    }
    // Update is called once per frame
    //OnPointerDown is also required to receive OnPointerUp callbacks
    public void OnPointerDown(PointerEventData eventData)
    {
        //mtext.text = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // m_Debug("抬起；");
       // DebugHelper.instance.Log("抬起");
        LogEvent("click text", eventData);
        mManager.Show(mInputType, mTextType);
    }

    private void LogEvent(string prefix, PointerEventData eventData)
    {
        Debug.Log(prefix + ": " + eventData.pointerCurrentRaycast.gameObject.name + " x=" + eventData.position.x + ",y=" + eventData.position.y);
    }



}
