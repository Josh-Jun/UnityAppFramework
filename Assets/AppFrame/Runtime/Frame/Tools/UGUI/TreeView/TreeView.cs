/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年8月13 17:53
 * function    :
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppFrame.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class TreeData
    {
        public int Id; //节点id
        public string Name; //节点名称
        public bool IsUnfold; //是否展开
        public bool IsChecked; //是否选中
        public int ParentId; //父节点id
        public int Layer; //层级
    }

    public class TreeView : MonoBehaviour
    {
        public bool isToggle = true;
        public int tabCount = 1;
        public int itemHeight = 20;
        public RectTransform itemParent;
        [HideInInspector]
        public GameObject item;
        public Color enterColor = Color.gray;
        public Color clickColor = Color.blue;

        private List<TreeData> _treeDates = new List<TreeData>();

        private Dictionary<int, GameObject> _treeItemsPairs = new Dictionary<int, GameObject>();

        private List<TreeData> selectItems = new List<TreeData>();

        private float width;

        public Action<List<TreeData>> RefreshTreeViewEvent;
        void Awake()
        {
            width = GetComponent<RectTransform>().rect.width;
            item.SetActive(false);
            // _treeDates = new List<TreeData>()
            // {
            //     new TreeData(){Id = 0, Layer = 0, Name = "test1", ParentId = -1},
            //     new TreeData(){Id = 1, Layer = 1, Name = "test2", ParentId = 0},
            //     new TreeData(){Id = 2, Layer = 2, Name = "test3", ParentId = 1},
            //     new TreeData(){Id = 3, Layer = 2, Name = "test4", ParentId = 1},
            //     new TreeData(){Id = 4, Layer = 1, Name = "test5", ParentId = 0},
            //     new TreeData(){Id = 5, Layer = 2, Name = "test6", ParentId = 4},
            //     new TreeData(){Id = 6, Layer = 3, Name = "test7", ParentId = 5},
            //     new TreeData(){Id = 7, Layer = 1, Name = "test8", ParentId = 0},
            //     new TreeData(){Id = 8, Layer = 2, Name = "test9", ParentId = 7},
            // };
            // InitTreeView(_treeDates);
        }

        public void InitTreeView(List<TreeData> treeDatas)
        {
            Clear();
            for (int i = 0; i < treeDatas.Count; i++)
            {
                var data = treeDatas[i];
                AddItem(data);
            }
            RefreshTreeView();
        }

        private void UnfoldChildItem(TreeData parentData)
        {
            parentData.IsUnfold = !parentData.IsUnfold;
            foreach (var data in _treeDates)
            {
                if (data.ParentId == parentData.Id)
                {
                    _treeItemsPairs[data.Id].SetActive(parentData.IsUnfold);
                }
            }
        }

        private void OnClickToggleEvent(bool value, TreeData parentData)
        {
            parentData.IsChecked = value;
            foreach (var data in _treeDates)
            {
                if (data.ParentId == parentData.Id)
                {
                    data.IsChecked = value;
                    _treeItemsPairs[data.Id].transform.Find("Root/Toggle").GetComponent<Toggle>().isOn = value;
                }
            }
        }

        public void RefreshTreeView()
        {
            foreach (var data in _treeDates)
            {
                var t = _treeItemsPairs[data.Id].transform;
                var color = t.childCount > 1 ? Color.white : Color.clear;
                t.Find("Root/Button").GetComponent<Image>().color = color;
            }
        }

        public List<TreeData> GetSelectItems()
        {
            return selectItems;
        }

        public List<TreeData> GetCheckedItems()
        {
            List<TreeData> datas = new List<TreeData>();
            foreach (var data in _treeDates)
            {
                if (data.IsChecked)
                {
                    datas.Add(data);
                }
            }
            return datas;
        }

        public void RemoveItem(int id)
        {
            Destroy(_treeItemsPairs[id]);
            TreeData td = null;
            foreach (var data in _treeDates)
            {
                if (data.Id == id)
                {
                    td = data;
                }
            }
            if (td != null)
            {
                _treeDates.Remove(td);
            }
        }

        public void AddItem(TreeData data)
        {
            var parent = data.ParentId < 0 ? itemParent : _treeItemsPairs[data.ParentId].transform;
            var go = Instantiate(item, parent);
            go.name = data.Id.ToString();
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, itemHeight);
            
            var btn_rt = rt.Find("Root/Button").GetComponent<RectTransform>();
            btn_rt.sizeDelta = Vector2.one * itemHeight;
            btn_rt.localEulerAngles = data.IsUnfold ? Vector3.zero : Vector3.forward * 90;
            var btn = rt.Find("Root/Button").GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                btn_rt.localEulerAngles = !data.IsUnfold ? Vector3.zero : Vector3.forward * 90;
                UnfoldChildItem(data);
            });
            
            var image_rt = rt.Find("Root/Image").GetComponent<RectTransform>();
            image_rt.sizeDelta = Vector2.one * itemHeight;
            
            var toggle_rt = rt.Find("Root/Toggle").GetComponent<RectTransform>();
            toggle_rt.sizeDelta = Vector2.one * itemHeight;
            var toggle = rt.Find("Root/Toggle").GetComponent<Toggle>();
            toggle.gameObject.SetActive(isToggle);
            toggle.isOn = data.IsChecked;
            toggle.onValueChanged.AddListener(value => { OnClickToggleEvent(value, data); });
            
            var layout = rt.Find("Root").GetComponent<HorizontalLayoutGroup>();
            layout.padding.left = (data.Layer+tabCount) * itemHeight;
            
            var text_rt = rt.Find("Root/Text").GetComponent<RectTransform>();
            var w = isToggle ? 
                width - 3 * (itemHeight + layout.spacing) - tabCount * itemHeight: 
                width - 2 * (itemHeight + layout.spacing) - tabCount * itemHeight;
            text_rt.sizeDelta = new Vector2(w, itemHeight);
            var text = rt.Find("Root/Text").GetComponent<Text>();
            text.text = data.Name;
            text.fontSize = itemHeight - 4;
            
            go.SetActive(data.ParentId < 0);
            if (!_treeItemsPairs.ContainsKey(data.Id))
            {
                _treeItemsPairs.TryAdd(data.Id, go);
            }

            var root = rt.Find("Root").GetComponent<Image>();
            AddEventTrigger(root.gameObject, EventTriggerType.PointerEnter, eventData =>
            {
                if (selectItems.Contains(data)) return;
                root.color = enterColor;
            });
            AddEventTrigger(root.gameObject, EventTriggerType.PointerClick, eventData =>
            {
                root.color = clickColor;
                for (int i = 0; i < selectItems.Count; i++)
                {
                    var img = _treeItemsPairs[selectItems[i].Id].transform.Find("Root").GetComponent<Image>();
                    img.color = Color.clear;
                }
                selectItems.Clear();
                selectItems.Add(data);
            });
            AddEventTrigger(root.gameObject, EventTriggerType.PointerExit, eventData =>
            {
                if (selectItems.Contains(data)) return;
                root.color = Color.clear;
            });
            AddEventTrigger(root.gameObject, EventTriggerType.BeginDrag, eventData =>
            {
                var pointerEventData = eventData as PointerEventData;
                var d = go.transform.Find("Root/Text").gameObject;
                dragItem = Instantiate(d, transform);
                dragItem.transform.position = pointerEventData.currentInputModule.input.mousePosition;
                dragData = data;
            });
            AddEventTrigger(root.gameObject, EventTriggerType.Drag, eventData =>
            {
                var pointerEventData = eventData as PointerEventData;
                if (dragItem && pointerEventData != null)
                {
                    dragItem.transform.position += new Vector3(pointerEventData.delta.x, pointerEventData.delta.y);
                }
            });
            AddEventTrigger(root.gameObject, EventTriggerType.EndDrag, eventData =>
            {
                var pointerEventData = eventData as PointerEventData;
                Destroy(dragItem);
                if (pointerEventData != null)
                {
                    if (pointerEventData.pointerEnter)
                    {
                        var pair = _treeItemsPairs.Where(kvp => kvp.Value == pointerEventData.pointerEnter.transform.parent.gameObject).ToList().First().Key;
                        dragData.ParentId = pair;
                        _treeItemsPairs[dragData.Id].transform.SetParent(_treeItemsPairs[pair].transform);
                        var enumerable = _treeDates.Where(d => d.Id == pair);
                        var dd = enumerable.First();
                        dragData.Layer = dd.Layer + 1;
                        var hlg = _treeItemsPairs[dragData.Id].transform.Find("Root").GetComponent<HorizontalLayoutGroup>();
                        hlg.padding.left = (dragData.Layer+tabCount) * itemHeight;
                        if (!dd.IsUnfold)
                        {
                            _treeItemsPairs[dd.Id].transform.Find("Root/Button").GetComponent<RectTransform>().localEulerAngles = !dd.IsUnfold ? Vector3.zero : Vector3.forward * 90;
                            UnfoldChildItem(dd);
                        }
                        RefreshTreeView();
                        RefreshTreeViewEvent?.Invoke(_treeDates);
                    }
                }
                dragData = null;
            });
        }

        private GameObject dragItem;
        private TreeData dragData;
        public void Clear()
        {
            foreach (RectTransform rt in itemParent)
            {
                Destroy(rt.gameObject);
            }
            _treeItemsPairs.Clear();
        }
        
        private void AddEventTrigger(GameObject obj, EventTriggerType eventType, UnityAction<BaseEventData> ua)
        {
            if (obj == null)
            {
                Log.E($"GameObject({obj.name}) is null!!!");
                return;
            }

            EventTrigger eventTrigger = obj.TryGetComponent<EventTrigger>();
            //eventTrigger.triggers = new List<EventTrigger.Entry>();

            UnityAction<BaseEventData> callback = new UnityAction<BaseEventData>(ua);

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(callback);
            eventTrigger.triggers.Add(entry);
        }
    }
}