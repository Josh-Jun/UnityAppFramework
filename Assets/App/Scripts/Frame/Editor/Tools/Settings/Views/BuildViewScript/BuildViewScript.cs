using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using App.Core.Helper;
using App.Editor.Helper;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace App.Editor.View
{
    [Serializable]
    public class ViewScriptData
    {
        public ViewMold mold;
        public int layer;
        public bool active;
        public List<ViewData> views = new List<ViewData>();
    }
    [Serializable]
    public class ViewData
    {
        public string name;
        public string path;
        public List<ComponentData> components = new List<ComponentData>();
    }
    [Serializable]
    public class ComponentData
    {
        public string name;
        public string type;
        public string fullname;
        public int eventType;
        public bool isPublic;
    }

    public class BuildViewScript : IToolkitEditor
    {
        private VisualTreeAsset view_item;
        private ObjectField uiGameObjectField;

        private readonly string scriptViewPath = $"{EditorHelper.BaseEditorPath()}/Tools/Settings/Views/BuildViewScript/ui_view_script.txt";
        private readonly string scriptLogicPath = $"{EditorHelper.BaseEditorPath()}/Tools/Settings/Views/BuildViewScript/ui_logic_script.txt";

        private readonly string cachePath = $"{Application.dataPath.Replace("Assets", "")}Data/cache/viewscript";
        
        private VisualElement parent;
        private VisualElement type;
        private VisualElement child;
        private VisualElement btns;
        private Foldout rootFoldout;
        private TreeView treeView;
        
        private DropdownField pathField;
        private DropdownField uiComponentField;

        private ViewData _selectedViewData;

        private ViewScriptData _viewScriptData = null;
        
        private GameObject rootGameObject;

        private bool isCreateLogic = false;

        public void OnCreate(VisualElement root)
        {
            //获取item
            view_item = EditorHelper.GetEditorWindowsAsset<VisualTreeAsset>($"BuildViewScript/view_item.uxml");
            //获取根节点Foldout
            rootFoldout = root.Q<Foldout>("UIView");
            
            type = root.Q<VisualElement>("Type");
            parent = root.Q<VisualElement>("Root");
            child = root.Q<VisualElement>("Child");
            btns = root.Q<VisualElement>("Btns");
            btns.Q<Label>().text = "请选择对象并选择需要操作的组件";
            type.style.display = DisplayStyle.None;
            parent.style.display = DisplayStyle.None;
            btns.style.display = DisplayStyle.None;
            
            root.Q<Toggle>("CreateLogic").value = isCreateLogic;
            root.Q<Toggle>("CreateLogic").RegisterCallback<ChangeEvent<bool>>((evt) => { isCreateLogic = evt.newValue; });

            //获取对象ObjectField，并进行初始化
            uiGameObjectField = root.Q<ObjectField>("UIGameObject");
            uiGameObjectField.objectType = typeof(GameObject);
            uiGameObjectField.RegisterCallback<ChangeEvent<Object>>((evt) =>
            {
                OnObjectFieldChange(evt.newValue as GameObject);
            });
            
            rootFoldout.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (!evt.newValue)
                {
                    btns.Q<Label>().text = "";
                    _selectedViewData = null;
                    RefreshView(_selectedViewData);
                }
                else
                {
                    btns.Q<Label>().text = "请选择对象并添加需要操作的组件";
                }
            });

            //获取按钮，并添加按钮事件
            root.Q<Button>("BtnBuild").clicked += BuildEvent;
            treeView = root.Q<TreeView>("TreeView");
            
            //删除和添加按钮设置图标和添加事件
            root.Q<Button>("btn_remove").style.backgroundImage =
                new StyleBackground((Texture2D)EditorGUIUtility.IconContent("d_CollabDeleted Icon").image);
            root.Q<Button>("btn_add").style.backgroundImage =
                new StyleBackground((Texture2D)EditorGUIUtility.IconContent("d_CollabCreate Icon").image);
            
            root.Q<Button>("btn_remove").clicked += () =>
            {
                if (_selectedViewData == null) return;
                _selectedViewData.components.RemoveAt(_selectedViewData.components.Count - 1);
                RefreshView(_selectedViewData);
            };
            root.Q<Button>("btn_add").clicked += () =>
            {
                if (_selectedViewData == null) return;
                var go = rootGameObject.transform.Find(_selectedViewData.path).gameObject;
                var components = GetComponents(go, _selectedViewData.components);
                if (_selectedViewData.components.Count < GetComponents(go).Count)
                {
                    _selectedViewData.components.Add(components[0]);
                    RefreshView(_selectedViewData);
                }
                else
                {
                    Log.I("组件数量已达到上限");
                }
            };
        }

        private ViewScriptData LoadViewData(string name)
        {
            if (!Directory.Exists(cachePath)) Directory.CreateDirectory(cachePath);
            if (!File.Exists($"{cachePath}/{name}.json")) return new ViewScriptData();
            var json = File.ReadAllText($"{cachePath}/{name}.json");
            var data = JsonUtility.FromJson<ViewScriptData>(json);
            return data;
        }

        public void OnUpdate()
        {
            
        }
        public void OnDestroy()
        {
            
        }

        private void BuildEvent()
        {
            var view_path = Path.Combine(Application.dataPath, scriptViewPath);
            var view_script = File.ReadAllText(view_path);
            var logic_path = Path.Combine(Application.dataPath, scriptLogicPath);
            var logic_script = File.ReadAllText(logic_path);

            var view_script_name = rootFoldout.text;
            var folder_name = rootFoldout.text.Replace("View", "");
            var script_path = Path.Combine(Application.dataPath, $"Scripts/{folder_name}");
            var view_script_path = Path.Combine(script_path, $"{view_script_name}.cs");
            var logic_script_name = $"{folder_name}Logic";
            var logic_script_path = Path.Combine(script_path, $"{logic_script_name}.cs");

            view_script = view_script.Replace("#VIEWMOLD#", $"{_viewScriptData.mold}");
            view_script = view_script.Replace("#ACTIVE#", $"{_viewScriptData.active}".ToLower());
            view_script = view_script.Replace("#LAYER#", $"{_viewScriptData.layer}");
            view_script = view_script.Replace("#NAMESPACE#", folder_name);
            view_script = view_script.Replace("#SCRIPTNAME#", view_script_name);
            view_script = view_script.Replace("#VARIABLE#", CreatePrivateVariableContent());
            view_script = view_script.Replace("#GETSET#", CreatePublicVariableContent());
            view_script = view_script.Replace("#INIT#", CreateInitContent());
            view_script = view_script.Replace("#REGISTER#", CreateRegisterContent());
            view_script = view_script.Replace("#OPEN#", "");
            view_script = view_script.Replace("#CLOSE#", "");

            logic_script = logic_script.Replace("#NAMESPACE#", folder_name);
            logic_script = logic_script.Replace("#SCRIPTNAME#", logic_script_name);
            
            if (!Directory.Exists(script_path))
            {
                Directory.CreateDirectory(script_path);
            }

            if (File.Exists(view_script_path))
            {
                File.Delete(view_script_path);
            }

            File.WriteAllText(view_script_path, view_script);
            if (isCreateLogic)
            {
                File.WriteAllText(logic_script_path, logic_script);
            }
            
            var json = JsonUtility.ToJson(_viewScriptData);
            File.WriteAllText($"{cachePath}/{view_script_name}.json", json);
            
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 当对象选择有变化时调用
        /// </summary>
        /// <param name="go"></param>
        private void OnObjectFieldChange(GameObject go)
        {
            RefreshView(null);
            _viewScriptData = null;
            rootGameObject = go;
            parent.style.display = go ? DisplayStyle.Flex : DisplayStyle.None;
            type.style.display = go ? DisplayStyle.Flex : DisplayStyle.None;
            btns.style.display = go ? DisplayStyle.Flex : DisplayStyle.None;
            if(go == null) return;
            _viewScriptData = LoadViewData(go.name);
            btns.Q<Label>().text = "请选择对象并添加需要操作的组件";
            rootFoldout.text = go.name;
            
            var view_mold = type.Q<EnumField>("ViewMold");
            view_mold.Init(_viewScriptData?.mold ?? ViewMold.UI2D);
            view_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (ViewMold)System.Enum.Parse(typeof(ViewMold), evt.newValue);
                _viewScriptData.mold = mold;
            });
            
            var layer = type.Q<IntegerField>("Layer");
            layer.value = _viewScriptData?.layer ?? 0;
            layer.RegisterCallback<ChangeEvent<int>>((evt) => { _viewScriptData.layer = evt.newValue; });
            
            var active = type.Q<Toggle>("Active");
            active.value = _viewScriptData?.active ?? false;
            active.RegisterCallback<ChangeEvent<bool>>((evt) => { _viewScriptData.active = evt.newValue; });
            
            var items = new List<TreeViewItemData<ViewData>>();

            foreach (Transform transform in go.transform)
            {
                var treeViewItemData = GetTreeViewItemData(transform);
                items.Add(treeViewItemData);
            }
            
            treeView.SetRootItems(items);
            treeView.makeItem = MakeTreeViewItem;
            treeView.bindItem = BindTreeViewItem;
            treeView.selectionChanged += OnItemsChosen;
            treeView.selectionType = SelectionType.Single;
            treeView.Rebuild();
        }

        private TreeViewItemData<ViewData> GetTreeViewItemData(Transform parent, string path = "")
        {
            var childTreeViewItemData = new List<TreeViewItemData<ViewData>>();
            var currentPath = path == string.Empty ? parent.name : path + "/" + parent.name;
            foreach (Transform transform in parent)
            {
                var itemData = GetTreeViewItemData(transform, currentPath);
                childTreeViewItemData.Add(itemData);
            }
            
            var uiViewData = new ViewData { path = currentPath, name = parent.name };
            var vd = _viewScriptData.views.FirstOrDefault(d=>d.path == currentPath);
            if (vd != null && vd.path == currentPath)
            {
                uiViewData = vd;
            }
            else
            {
                _viewScriptData.views.Add(uiViewData);
            }

            return new TreeViewItemData<ViewData>(parent.GetInstanceID(), uiViewData, childTreeViewItemData);
        }

        private VisualElement MakeTreeViewItem()
        {
            var item = new Label { style = { marginTop = 2 }};
            return item;
        }
        private void BindTreeViewItem(VisualElement ve, int index)
        {
            var item = ve as Label;
            var data = treeView.GetItemDataForIndex<ViewData>(index);
            if (item != null)
            {
                item.text = data.name;
            }
        }
        private void OnItemsChosen(IEnumerable<object> objs)
        {
            var enumerable = objs as object[] ?? objs.ToArray();
            if(!enumerable.Any()) return;
            if (enumerable.First() is not ViewData data) return;
            _selectedViewData = data;
            btns.Q<Label>().text = data.path;
            RefreshView(data);
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        private void RefreshView(ViewData data)
        {
            child.Clear();
            if(data == null) return;
            for (var i = 0; i < data.components.Count; i++)
            {
                var index = i;
                VisualElement item = view_item.CloneTree();
                //设置itemId
                item.Q<Label>("Count").text = index.ToString();
                var go = rootGameObject.transform.Find(data.path).gameObject;
                //获取当前对象上的所有组件
                var _components = GetComponents(go);
                //将组件转换成下拉框的选项
                var names = _components.Select(t => t.fullname).ToList();

                //设置组件下拉框的值，并添加相应事件
                var dropdownField = item.Q<DropdownField>("UIComponent");
                dropdownField.choices = names;
                dropdownField.value = data.components[index].fullname;
                
                dropdownField.RegisterCallback<ChangeEvent<string>>((evt) =>
                {
                    data.components[index] = _components[names.IndexOf(evt.newValue)];
                });
                
                child.Add(item);
            }
        }

        /// <summary>
        /// 获取UI组件
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private List<ComponentData> GetComponents(GameObject go, List<ComponentData> cds = null)
        {
            var components = new List<ComponentData>();
            var _components = go.GetComponents<Component>();
            foreach (var component in _components)
            {
                //剔除CanvasRenderer
                if (component is CanvasRenderer)
                {
                    continue;
                }
                
                var data = new ComponentData
                {
                    name = go.name,
                    type = component.GetType().Name,
                    fullname = component.GetType().FullName,
                    eventType = IsEvent(component) ? component is UnityEngine.UI.Button ? 1 : 2 : 0,
                    isPublic = IsPublic(component)
                };
                if (cds != null)
                {
                    var result = cds.Any(cd => data.fullname == cd.fullname);
                    if(result) continue;
                }
                components.Add(data);
            }
            return components;
        }

        private string CreatePrivateVariableContent()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var uiViewData in _viewScriptData.views)
            {
                for (int i = 0; i < uiViewData.components.Count; i++)
                {
                    var type = uiViewData.components[i].type;
                    var name = $"{uiViewData.name.ToLower()}{type}";
                    string str = $"\t\tprivate {type} {name};";
                    sb.AppendLine(str);
                }
            }
        
            return sb.ToString();
        }
        
        private string CreatePublicVariableContent()
        {
            var sb = new StringBuilder();
            foreach (var uiViewData in _viewScriptData.views)
            {
                foreach (var t in uiViewData.components)
                {
                    if (t.isPublic)
                    {
                        var name = $"{t.name.ToLower()}{t.type}";
                        var nameUpperCase = $"{ToUpperCase(name)}";
                        string str = $"\t\tpublic {t.type} {nameUpperCase} {{ get {{ return {name}; }} }}";
                        sb.AppendLine(str);
                    }
                }
            }
            return sb.ToString();
        }
        
        
        private string CreateRegisterContent()
        {
            var sb = new StringBuilder();
            foreach (var uiViewData in _viewScriptData.views)
            {
                foreach (var t in uiViewData.components)
                {
                    var str = "";
                    var tName = t.type;
                    var name = $"{uiViewData.name.ToLower()}{tName}";
                    str = t.eventType switch
                    {
                        1 =>
                            $"\t\t\t{name}.onClick.AddListener(() => {{ SendEventMsg(\"{ToUpperCase(name)}Event\"); }});",
                        2 =>
                            $"\t\t\t{name}.onValueChanged.AddListener((arg) => {{ SendEventMsg(\"{ToUpperCase(name)}Event\", arg); }});",
                        _ => str
                    };
                    if (!string.IsNullOrEmpty(str))
                    {
                        sb.AppendLine(str);
                    }
                }
            }
        
            return sb.ToString();
        }
        
        private string CreateInitContent()
        {
            var sb = new StringBuilder();
            foreach (var uiViewData in _viewScriptData.views)
            {
                foreach (var t in uiViewData.components)
                {
                    var name = $"{t.name.ToLower()}{t.type}";
                    var path = $"\"{uiViewData.path}\"";
                    var str = $"\t\t\t{name} = this.FindComponent<{t.type}>({path});";
                    sb.AppendLine(str);
                }
            }
        
            return sb.ToString();
        }

        private string ToUpperCase(string str)
        {
            var _str = "";
            for (int i = 0; i < str.Length; i++)
            {
                var s = str[i].ToString();
                if (i == 0)
                {
                    s = s.ToUpper();
                }

                _str += s;
            }

            return _str;
        }

        private bool IsPublic(Component component)
        {
            return component is 
                UnityEngine.UI.Text or 
                UnityEngine.UI.InputField or 
                UnityEngine.UI.Dropdown or 
                UnityEngine.UI.Slider or 
                UnityEngine.UI.Toggle or 
                UnityEngine.UI.Image or 
                UnityEngine.UI.RawImage or 
                UnityEngine.UI.GridLayoutGroup or 
                UnityEngine.UI.HorizontalLayoutGroup or 
                UnityEngine.UI.VerticalLayoutGroup or 
                UnityEngine.UI.ContentSizeFitter or 
                UnityEngine.CanvasGroup or 
                UnityEngine.Canvas;
        }

        private bool IsEvent(Component component)
        {
            return component is 
                UnityEngine.UI.Button or 
                UnityEngine.UI.InputField or 
                UnityEngine.UI.Dropdown or 
                UnityEngine.UI.Slider or 
                UnityEngine.UI.Toggle or 
                UnityEngine.UI.ScrollRect or 
                UnityEngine.UI.Scrollbar;
        }
    }
}