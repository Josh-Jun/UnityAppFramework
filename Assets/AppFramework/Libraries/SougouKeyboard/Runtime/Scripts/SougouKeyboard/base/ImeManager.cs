using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImeManager : MonoBehaviour
{
    public ImeDelegateBase mDelegate;
    private ImeBase mIme;
    private Vector2 mSize;

    void Start()
    {
        GetImeBase();
    }

    void Update()
    {
        if (null == mIme)
        {
            return;
        }
        mIme.UpdateData();
    }

    void GetImeBase()
    {
        if (null != mDelegate)
        {
#if UNITY_EDITOR
            mIme = new DummyIme();
#else
            mIme = new SGIme();
#endif
            mIme.Create(mDelegate);
        }
#if UNITY_EDITOR
        Hide();
#endif
    }

    //export
    public void Show(SGImeInputType typeInput, SGImeTextType typeText)
    {
        if (mIme == null)
        {
            GetImeBase();
        }
        //DebugHelper.instance.Log("Imemanager.show()"); check right
        mIme.Show(typeInput, typeText);
        mIme.GetSize(ref mSize);
    }

    public void Hide()
    {
        if (mIme != null)
            mIme.Hide();
        if (mDelegate != null)
            mDelegate.OnIMEHide();
    }

    public void Draw(Texture2D tex)
    {
        mIme.Draw(tex);
    }

    public void OnTouch(float x, float y, SGImeMotionEventType type)
    {
        mIme.OnTouch(x, y, type);
    }
}
