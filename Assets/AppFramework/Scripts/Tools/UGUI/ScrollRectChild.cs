using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 解决2个层级的ScrollView嵌套但是方向不同的情况
/// </summary>
public class ScrollRectChild : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private ScrollRect _parent;
    private ScrollRect self;

    private enum Direction
    {
        Horizontal,
        Vertical
    }

    // 滑动方向
    private Direction _direction;
    // 操作方向
    private Direction _beginDragDirection;

    public void Awake()
    {
        self = GetComponent<ScrollRect>();
        if (transform.parent)
        {
            _parent = transform.parent.GetComponentInParent<ScrollRect>();
        }
        _direction = self.horizontal ? Direction.Horizontal : Direction.Vertical;
    }
    
    // 开始拖动时已选择控制柄
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_parent)
        {
            _beginDragDirection = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y) ? Direction.Horizontal : Direction.Vertical;
            if (_beginDragDirection != _direction)
            {
                // 当前操作方向不等于滑动方向，将事件传给父对象
                ExecuteEvents.Execute(_parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
                return;
            }
        }
    }

    // 拖动内容时的处理
    public void OnDrag(PointerEventData eventData)
    {
        if (_parent)
        {
            if (_beginDragDirection != _direction)
            {
                // 当前操作方向不等于滑动方向，将事件传给父对象
                ExecuteEvents.Execute(_parent.gameObject, eventData, ExecuteEvents.dragHandler);
                return;
            }
        }
    }

    // 完成内容拖动时的处理
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_parent)
        {
            if (_beginDragDirection != _direction)
            {
                // 当前操作方向不等于滑动方向，将事件传给父对象
                ExecuteEvents.Execute(_parent.gameObject, eventData, ExecuteEvents.endDragHandler);
                return;
            }
        }
    }

    public void OnScroll(PointerEventData data)
    {
        if (_parent)
        {
            if (_beginDragDirection != _direction)
            {
                // 当前操作方向不等于滑动方向，将事件传给父对象
                ExecuteEvents.Execute(_parent.gameObject, data, ExecuteEvents.scrollHandler);
                return;
            }
        }
    }
}