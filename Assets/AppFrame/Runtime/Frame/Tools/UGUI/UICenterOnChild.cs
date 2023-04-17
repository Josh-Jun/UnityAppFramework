using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
/// <summary>
/// UGUI ScrollRect 滑动元素居中
/// </summary>
namespace AppFrame.Tools
{
    public class UICenterOnChild : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public float scrollSpeed = 8f;
        public float scrollDistance = 200f;

        private ScrollRect scrollRect;
        private float[] pageArray;
        private float targetPagePosition = 0f;
        private bool isDrag = false;
        private int pageCount;
        private int currentPage = 0;
        private List<Transform> items = new List<Transform>();

        /// <summary> 获取当前页码 </summary>
        public int GetCurrentPageIndex
        {
            get { return currentPage; }
            private set { }
        }

        /// <summary> 获取总页数 </summary>
        public int GetTotalPages
        {
            get { return pageCount; }
            private set { }
        }

        public delegate void IntValueDelegate(int value);

        public event IntValueDelegate OnDragEndEvent;

        // Use this for initialization
        void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            InitPageArray();
        }

        /// <summary>
        /// 初始化获取元素总个数
        /// </summary>
        public void InitPageArray()
        {
            foreach (Transform item in scrollRect.content)
            {
                if (item.gameObject.activeSelf && !items.Contains(item))
                {
                    items.Add(item);
                }
            }

            pageCount = items.Count;
            pageArray = new float[pageCount];
            for (int i = 0; i < pageCount; i++)
            {
                pageArray[i] = (1f / (pageCount - 1)) * i;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isDrag)
            {
                if (scrollRect.horizontal)
                {
                    scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition,
                        targetPagePosition, scrollSpeed * Time.deltaTime);
                }
                else if (scrollRect.vertical)
                {
                    scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition,
                        targetPagePosition, scrollSpeed * Time.deltaTime);
                }
            }
        }

        private Vector2 beginPos;

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDrag = true;
            beginPos = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDrag = false;

            //float pos = scrollRect.horizontal ? scrollRect.horizontalNormalizedPosition : scrollRect.verticalNormalizedPosition;
            //int index = 0;
            //float offset = Math.Abs(pageArray[index] - pos);
            //for (int i = 1; i < pageArray.Length; i++)
            //{
            //    float _offset = Math.Abs(pageArray[i] - pos);
            //    if (_offset < offset)
            //    {
            //        index = i;
            //        offset = _offset;
            //    }
            //}
            //targetPagePosition = pageArray[index];
            //currentPage = index;
            //OnDragEndEvent?.Invoke(currentPage);

            Vector2 dirV2 = eventData.position - beginPos;
            var dir = scrollRect.horizontal ? dirV2.x > 0 : dirV2.y > 0;
            var dis = scrollRect.horizontal ? dirV2.x : dirV2.y;
            if (Math.Abs(dis) > scrollDistance)
            {
                if (dir)
                {
                    ToPrevious((int page) => { OnDragEndEvent?.Invoke(page); });
                }
                else
                {
                    ToNext((int page) => { OnDragEndEvent?.Invoke(page); });
                }
            }
        }

        /// <summary>
        /// 向前移动一个元素
        /// </summary>
        public void ToPrevious(Action<int> callback = null)
        {
            if (currentPage > 0)
            {
                currentPage -= 1;
                targetPagePosition = pageArray[currentPage];
            }

            callback?.Invoke(currentPage);
        }

        /// <summary>
        /// 向后移动一个元素
        /// </summary>
        public void ToNext(Action<int> callback = null)
        {
            if (currentPage < pageCount - 1)
            {
                currentPage += 1;
                targetPagePosition = pageArray[currentPage];
            }

            callback?.Invoke(currentPage);
        }

        /// <summary>
        /// 设置当前页码
        /// </summary>
        /// <param name="index"></param>
        public void SetCurrentPageIndex(int index)
        {
            currentPage = index;
            targetPagePosition = pageArray[currentPage];
        }
    }
}