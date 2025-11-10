/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年11月7 15:37
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectDragRefresh : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        private ScrollRect scrollRect;
        private bool isDrag = false;
        
        public UnityEvent OnLeftDragRefresh;
        public UnityEvent OnRightDragRefresh;
        public UnityEvent OnUpDragRefresh;
        public UnityEvent OnDownDragRefresh;
        
        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDrag = true;
            scrollRect.OnBeginDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            scrollRect.OnEndDrag(eventData);
            if (!isDrag) return;
            if (scrollRect.horizontal)
            {
                switch (scrollRect.horizontalNormalizedPosition)
                {
                    case > 1.5f:
                        OnLeftDragRefresh?.Invoke();
                        break;
                    case < -0.5f:
                        OnRightDragRefresh?.Invoke();
                        break;
                }
            }
            else
            {
                switch (scrollRect.verticalNormalizedPosition)
                {
                    case > 1.5f:
                        OnDownDragRefresh?.Invoke();
                        break;
                    case < -0.5f:
                        OnUpDragRefresh?.Invoke();
                        break;
                }
            }
        }
    }
}
