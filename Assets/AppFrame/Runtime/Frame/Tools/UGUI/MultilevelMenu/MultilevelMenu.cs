/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年8月21 15:12
 * function    :
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using AppFrame.Tools;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class MultilevelMenu : MonoBehaviour
    {
        public GameObject root;
        public GameObject item;

        public Color selectColor = Color.blue;

        private List<string> _menuPathList = new List<string>();

        public List<string> MenuPathList
        {
            get => _menuPathList;
            set
            {
                _menuPathList = value;
                InitMultilevelMenu();
            }
        }

        private GameObject _rootMenu;

        private Dictionary<string, GameObject> _multilevelMenu = new Dictionary<string, GameObject>();

        public Action<string> OnSelect = null;

        private RectTransform[] children = null;
        
        private List<string> foots = new List<string>();

        private void Awake()
        {
            root.SetActive(false);
            item.SetActive(false);
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if(_rootMenu == null) return;
                _rootMenu.SetActive(!_rootMenu.activeSelf);
            });

            // List<string> menus = new List<string>();
            // menus.Add("Empty");
            // menus.Add("GameObject/Cube");
            // menus.Add("GameObject/Sphere");
            // menus.Add("GameObject/Capsule");
            // menus.Add("GameObject/Cylinder");
            // menus.Add("GameObject/Plane");
            // menus.Add("GameObject/Quad");
            // menus.Add("Light/Directional Light");
            // menus.Add("Light/Point Light");
            // menus.Add("Light/Spot Light");
            // menus.Add("1/2/3/44444");
            // MenuPathList = menus;
        }

        private void InitMultilevelMenu()
        {
            if(_menuPathList == null || _menuPathList.Count == 0) return;
            List<string> root_menu = new List<string>();
            Dictionary<string, List<string>> childPairs = new Dictionary<string, List<string>>();
            foreach (var menu in _menuPathList)
            {
                string[] str = menu.Split('/');
                var _str_ = "";
                for (int i = 0; i < str.Length; i++)
                {
                    var _str = str[i];
                    if (i == 0)
                    {
                        if (!root_menu.Contains(_str))
                        {
                            root_menu.Add(_str);
                        }
                    }
                    else
                    {
                        var s = i == 1 ? "" : "/";
                        _str_ += s + str[i - 1];
                        if (childPairs.ContainsKey(_str_))
                        {
                            childPairs[_str_].Add(_str);
                        }
                        else
                        {
                            List<string> childs = new List<string>();
                            childs.Add(_str);
                            childPairs.Add(_str_, childs);
                        }
                    }
                }                
                foots.Add(str[str.Length - 1]);
            }

            _rootMenu = CreateMenu(root_menu);
            _rootMenu.name = "RootMenu";
            _rootMenu.SetActive(false);

            foreach (var childStr in childPairs)
            {
                var child = CreateMenu(childStr.Value, childStr.Key);
                child.name = childStr.Key;
                child.SetActive(false);
                _multilevelMenu.TryAdd(childStr.Key, child);
            }
            children = gameObject.GetComponentsInChildren<RectTransform>();
        }
        
        private GameObject CreateMenu(List<string> list, string parent = null)
        {
            var go = Instantiate(root, transform, false);
            go.SetActive(true);
            foreach (var str in list)
            {
                var child = Instantiate(item, go.transform, false);
                var s = parent == null ? "" : "/";
                child.name = parent + s + str;
                child.transform.Find("Text").GetComponent<Text>().text = str;
                child.transform.Find("Image").gameObject.SetActive(!foots.Contains(str));
                child.SetActive(true);
                AddEventTrigger(child, EventTriggerType.PointerEnter, data =>
                {
                    OpenMultilevelMenu(data);
                    child.GetComponent<Image>().color = selectColor;
                });
                AddEventTrigger(child, EventTriggerType.PointerExit, data =>
                {
                    child.GetComponent<Image>().color = Color.clear;
                });
                if (foots.Contains(str))
                {
                    AddEventTrigger(child, EventTriggerType.PointerClick, data =>
                    {
                        CLoseAllMenu();
                        OnSelect?.Invoke(str);
                    });
                }
            }

            return go;
        }

        private void OpenMultilevelMenu(BaseEventData data)
        {
            var eventData = data as PointerEventData;
            if (eventData == null) return;
            var go = eventData.pointerEnter;
            if(go == null) return;
            foreach (var menu in _multilevelMenu)
            {
                if (!go.name.Contains(menu.Key))
                {
                    menu.Value.SetActive(false);
                }
            }
            if (!_multilevelMenu.ContainsKey(go.name)) return;
            var rt = go.GetComponent<RectTransform>();
            var pos = rt.position;
            var layout = go.transform.parent.GetComponent<VerticalLayoutGroup>();
            var offset = (rt.rect.width / 2 + layout.padding.left) * Vector3.right + (rt.rect.height / 2 + layout.padding.top) * Vector3.up;
            _multilevelMenu[go.name].GetComponent<RectTransform>().position = pos + offset;
            _multilevelMenu[go.name].SetActive(true);
            children = gameObject.GetComponentsInChildren<RectTransform>();
        }
       
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    CLoseAllMenu();
                }
                else
                {
                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.position = Input.mousePosition;
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventData, results);
                    if (!IsPointerOverMyself(results))
                    {
                        CLoseAllMenu();
                    }
                }
            }
        }

        private bool IsPointerOverMyself(List<RaycastResult> results)
        {
            bool isPointerOverMyself = false;
            foreach (var child in children)
            {
                foreach (var result in results)
                {
                    if (child.gameObject == result.gameObject)
                    {
                        isPointerOverMyself = true;
                        break;
                    }
                }
            }
            return isPointerOverMyself;
        }

        private void CLoseAllMenu()
        {
            foreach (var menu in _multilevelMenu)
            {
                menu.Value.SetActive(false);
            }
            _rootMenu.SetActive(false);
        }

        private static void AddEventTrigger(GameObject obj, EventTriggerType eventType, UnityAction<BaseEventData> ua)
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