/* *
 * ===============================================
 * author      : Josh@macbook
 * e-mail      : shijun_z@163.com
 * create time : 2025年3月17 16:43
 * function    : 
 * ===============================================
 * */

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum JoystickType
{
    Fixed,      //固定式摇杆
    Floating,   //浮动式摇杆(根据点击屏幕的位置生成摇杆控制器)
    Dynamic     //动态摇杆(摇杆可以被动态拖拽)
}
public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public JoystickType joystickType;

    public Vector2 input = Vector2.zero;

    public Action<Vector2> OnJoystickMoveEvent;

    private RectTransform background;
    private RectTransform handler;
    private RectTransform self = null;
    private Canvas canvas;

    private bool isDragging = false;
    private Vector2 radius;
    private void Awake()
    {
        background = transform.Find("Background").GetComponent<RectTransform>();
        handler = transform.Find("Background/Handler").GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        self = GetComponent<RectTransform>();
        if (joystickType != JoystickType.Fixed)
            background.gameObject.SetActive(false);
        radius = background.sizeDelta / 2;
    }
#if UNITY_EDITOR
    private float x, y, _x, _y;
#endif
    public void Update()
    {
#if UNITY_EDITOR
        if (!background.gameObject.activeSelf) return;
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        if (x != 0 || y != 0)
        {
            isDragging = true;
            input = new Vector2(x, y);
            _x = x; _y = y;
            SetHandlerPos();
            if (joystickType != JoystickType.Fixed)
                background.gameObject.SetActive(true);
        }
        if ((_x != 0 || _y != 0) && x == 0 && y == 0)
        {
            isDragging = false;
            input = Vector2.zero;
            _x = 0; _y = 0;
            SetHandlerPos();
            if (joystickType != JoystickType.Fixed)
                background.gameObject.SetActive(false);
        }
#endif
        if (isDragging)
        {
            OnJoystickMoveEvent?.Invoke(input);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (joystickType != JoystickType.Fixed)
        {
            background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            background.gameObject.SetActive(true);
        }
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true;
        //将屏幕中的触点和background的距离映射到ui空间下实际的距离
        var eventPostion = ScreenPointToAnchoredPosition(eventData.position);
        input = (eventPostion - background.anchoredPosition) / (radius * canvas.scaleFactor);
        // 动态摇杆(摇杆可以被动态拖拽)设置拖拽位置
        if (joystickType == JoystickType.Dynamic && input.magnitude > 1)
        {
            var difference = input.normalized * (input.magnitude - 1) * radius;
            background.anchoredPosition += difference;
        }

        SetHandlerPos();
    }

    private void SetHandlerPos()
    {
        //计算摇杆坐标
        var magnitude = input.magnitude;
        magnitude = Mathf.Clamp(magnitude, 0, 1);
        input = input.normalized * magnitude;
        //实时计算handle的位置
        handler.anchoredPosition = input * radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (joystickType != JoystickType.Fixed)
            background.gameObject.SetActive(false);
        input = Vector2.zero;
        handler.anchoredPosition = Vector2.zero;
        isDragging = false;
    }

    private Vector2 ScreenPointToAnchoredPosition(Vector2 postion)
    {
        var _camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(self, postion, _camera, out var point);
        // 根据self的pivot计算偏移量
        var offset = new Vector2((self.pivot.x + 0.5f) * self.sizeDelta.x, (self.pivot.y - 0.5f) * self.sizeDelta.y);
        return point - offset;
    }
}
