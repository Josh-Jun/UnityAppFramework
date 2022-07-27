using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class GridLayourCellSizeFitter : MonoBehaviour
{
    private GridLayoutGroup self;
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        
    }

    public void Init()
    {
        self = GetComponent<GridLayoutGroup>();
        Rect rect = (transform.parent as RectTransform).rect;
        self.cellSize = new Vector2(rect.width, rect.height);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(GridLayourCellSizeFitter))]
public class GridLayourCellSizeFitterEditor : Editor
{
    private GridLayourCellSizeFitter fitter;
    void OnEnable()
    {
        fitter = (GridLayourCellSizeFitter)(target);
        fitter.Init();
    }
}
#endif