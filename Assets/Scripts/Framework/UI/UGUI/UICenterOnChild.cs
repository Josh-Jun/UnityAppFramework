using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
/// <summary>
/// UGUI ScrollRect 滑动元素居中
/// </summary>
public class UICenterOnChild : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public float scrollSpeed = 8f;
    public Transform UIGrid;
    private ScrollRect scrollRect;
    private float[] pageArray;
    private float targetPagePosition = 0f;
    private bool isDrag = false;
    private int pageCount;
    private int currentPage = 0;
    private List<Transform> items = new List<Transform>();
    // Use this for initialization
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }
    void Start()
    {
        InitPageArray();
    }
    /// <summary>
    /// 初始化获取元素总个数
    /// </summary>
    void InitPageArray()
    {
        foreach (Transform item in UIGrid)
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
                scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetPagePosition, scrollSpeed * Time.deltaTime);
            }
            else if (scrollRect.vertical)
            {
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetPagePosition, scrollSpeed * Time.deltaTime);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        float pos = scrollRect.horizontal ? scrollRect.horizontalNormalizedPosition : scrollRect.verticalNormalizedPosition;
        int index = 0;
        float offset = Math.Abs(pageArray[index] - pos);
        for (int i = 1; i < pageArray.Length; i++)
        {
            float _offset = Math.Abs(pageArray[i] - pos);
            if (_offset < offset)
            {
                index = i;
                offset = _offset;
            }
        }
        targetPagePosition = pageArray[index];
        currentPage = index;
    }
    /// <summary>
    /// 向左移动一个元素
    /// </summary>
    public void ToLeft()
    {
        if (currentPage > 0)
        {
            currentPage = currentPage - 1;
            targetPagePosition = pageArray[currentPage];
        }
    }
    /// <summary>
    /// 向右移动一个元素
    /// </summary>
    public void ToRight()
    {
        if (currentPage < pageCount - 1)
        {
            currentPage = currentPage + 1;
            targetPagePosition = pageArray[currentPage];
        }
    }
    /// <summary>
    /// 获取当前页码
    /// </summary>
    /// <returns></returns>
    public int GetCurrentPageIndex()
    {
        return currentPage;
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
    /// <summary>
    /// 获取总页数
    /// </summary>
    /// <returns></returns>
    public int GetTotalPages()
    {
        return pageCount;
    }
}