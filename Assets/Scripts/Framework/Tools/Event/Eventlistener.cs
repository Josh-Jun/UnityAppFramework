using UnityEngine;
using UnityEngine.EventSystems;

namespace EventListener
{
    public class Eventlistener : EventTrigger
    {
        public delegate void VoidDelegate(GameObject go);
        public delegate void VectorDelegate(GameObject go, Vector2 screenPosition);
        public delegate void BoolDelegate(GameObject go, bool value);
        public VoidDelegate onClick;//点击事件
        public VoidDelegate onDown;//按下事件
        public VoidDelegate onUp;//抬起事件
        public BoolDelegate onHover;//进入离开事件(true为进入，false为离开)
        public VectorDelegate onBeginDrag;//开始拖拽事件
        public VectorDelegate onDrag;//拖拽中事件
        public VectorDelegate onEndDrag;//结束拖拽事件

        static public Eventlistener Get(GameObject go)
        {
            PhysicsRaycaster raycaster = Camera.main.gameObject.GetComponent<PhysicsRaycaster>();
            if (raycaster == null) raycaster = Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
            Eventlistener listener = go.GetComponent<Eventlistener>();
            if (listener == null) listener = go.AddComponent<Eventlistener>();
            return listener;
        }
        static public Eventlistener Get(Component com)
        {
            PhysicsRaycaster raycaster = Camera.main.gameObject.GetComponent<PhysicsRaycaster>();
            if (raycaster == null) raycaster = Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
            Eventlistener listener = com.GetComponent<Eventlistener>();
            if (listener == null) listener = com.gameObject.AddComponent<Eventlistener>();
            return listener;
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null) onClick.Invoke(gameObject);
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null) onDown.Invoke(gameObject);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onUp != null) onUp.Invoke(gameObject);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onHover != null) onHover.Invoke(gameObject, true);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onHover != null) onHover.Invoke(gameObject, false);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag.Invoke(gameObject, eventData.position);
        }
        public override void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null) onDrag.Invoke(gameObject, eventData.position);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) onEndDrag.Invoke(gameObject, eventData.position);
        }
    }
}