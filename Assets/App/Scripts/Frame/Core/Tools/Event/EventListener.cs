using UnityEngine;
using UnityEngine.EventSystems;

namespace App.Core.Tools
{
    public class EventListener : 
        MonoBehaviour, 
        IPointerEnterHandler, 
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
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

        public static EventListener Get(GameObject go)
        {
            // if (go.layer != LayerMask.NameToLayer("UI"))
            //     go.layer = LayerMask.NameToLayer("UI");
            // var raycaster = Camera.main?.gameObject.GetComponent<PhysicsRaycaster>();
            // if (!raycaster) Camera.main?.gameObject.AddComponent<PhysicsRaycaster>();

            var listener = go.GetComponent<EventListener>();
            if (!listener) listener = go.AddComponent<EventListener>();
            return listener;
        }
        public static EventListener Get(Component com)
        {
            // if (com.gameObject.layer != LayerMask.NameToLayer("UI"))
            //     com.gameObject.layer = LayerMask.NameToLayer("UI");
            // var raycaster = Camera.main?.gameObject.GetComponent<PhysicsRaycaster>();
            // if (!raycaster) Camera.main?.gameObject.AddComponent<PhysicsRaycaster>();

            var listener = com.GetComponent<EventListener>();
            if (!listener) listener = com.gameObject.AddComponent<EventListener>();
            return listener;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(gameObject);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            onDown?.Invoke(gameObject);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            onUp?.Invoke(gameObject);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            onHover?.Invoke(gameObject, true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            onHover?.Invoke(gameObject, false);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(gameObject, eventData.position);
        }
        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(gameObject, eventData.delta);
        }
        public  void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(gameObject, eventData.position);
        }
    }
}