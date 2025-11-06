using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using App.Core.Helper;
using App.Core.Tools;
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

        private readonly string scriptViewPath =
            $"{EditorHelper.BaseEditorPath()}/Tools/Settings/Views/BuildViewScript/ui_view_script.txt";

        private readonly string scriptLogicPath =
            $"{EditorHelper.BaseEditorPath()}/Tools/Settings/Views/BuildViewScript/ui_logic_script.txt";

        private readonly string cachePath = $"{Application.dataPath.Replace("Assets", "")}Data/cache/viewscript";

        private VisualElement parent;
        private VisualElement type;
        private VisualElement child;
        private VisualElement buttons;
        private Foldout rootFoldout;
        private TreeView treeView;

        private DropdownField pathField;
        private DropdownField uiComponentField;

        private ViewData _selectedViewData;

        private ViewScriptData _viewScriptData;

        private GameObject rootGameObject;

        private bool isCreateLogic;

        public void OnCreate(VisualElement root)
        {
            //获取item
            view_item = EditorHelper.GetEditorWindowsAsset<VisualTreeAsset>($"BuildViewScript/view_item.uxml");
            //获取根节点Foldout
            rootFoldout = root.Q<Foldout>("UIView");

            type = root.Q<VisualElement>("Type");
            parent = root.Q<VisualElement>("Root");
            child = root.Q<VisualElement>("Child");
            buttons = root.Q<VisualElement>("Btns");
            buttons.Q<Label>().text = "请选择对象并选择需要操作的组件";
            type.style.display = DisplayStyle.None;
            parent.style.display = DisplayStyle.None;
            buttons.style.display = DisplayStyle.None;

            root.Q<Toggle>("CreateLogic").value = isCreateLogic;
            root.Q<Toggle>("CreateLogic")
                .RegisterCallback<ChangeEvent<bool>>((evt) => { isCreateLogic = evt.newValue; });

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
                    buttons.Q<Label>().text = "";
                    _selectedViewData = null;
                    RefreshView(_selectedViewData);
                }
                else
                {
                    buttons.Q<Label>().text = "请选择对象并添加需要操作的组件";
                }
            });

            //获取按钮，并添加按钮事件
            root.Q<Button>("BtnBuild").clicked += BuildEvent;
            root.Q<Button>("BtnSaveData").clicked += SaveData;
            root.Q<Button>("BtnClearData").clicked += ClearData;
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

        private void SaveData()
        {
            var view_script_name = rootFoldout.text;

            var json = JsonUtility.ToJson(_viewScriptData, true);
            File.WriteAllText($"{cachePath}/{view_script_name}.json", json);
        }

        private void ClearData()
        {
            var view_script_name = rootFoldout.text;
            var file = $"{cachePath}/{view_script_name}.json";
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            OnObjectFieldChange(rootGameObject);
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
            view_script = view_script.Replace("#MODULE#", folder_name);
            view_script = view_script.Replace("#SCRIPTNAME#", view_script_name);
            view_script = view_script.Replace("#VARIABLE#", CreatePublicVariableContent());
            view_script = view_script.Replace("#INIT#", CreateInitContent());
            view_script = view_script.Replace("#REGISTER#", CreateRegisterContent());
            view_script = view_script.Replace("#OPEN#", "");
            view_script = view_script.Replace("#CLOSE#", "");

            logic_script = logic_script.Replace("#MODULE#", folder_name);
            logic_script = logic_script.Replace("#SCRIPTNAME#", logic_script_name);
            logic_script = logic_script.Replace("#EVENT#", CreateEvent());

            var files = EditorHelper.GetFiles(Path.Combine(Application.dataPath, $"Scripts"), "cs");
            var logic = files.FirstOrDefault(f => f.Name == $"{logic_script_name}.cs");
            if (logic != null) logic_script_path = logic.FullName;
            var view = files.FirstOrDefault(f => f.Name == $"{view_script_name}.cs");
            if (view != null) view_script_path = view.FullName;

            if (view == null)
            {
                if (!Directory.Exists(script_path))
                {
                    Directory.CreateDirectory(script_path);
                }
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

            var json = JsonUtility.ToJson(_viewScriptData, true);
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
            buttons.style.display = go ? DisplayStyle.Flex : DisplayStyle.None;
            if (!go) return;
            _viewScriptData = LoadViewData(go.name);
            buttons.Q<Label>().text = "请选择对象并添加需要操作的组件";
            rootFoldout.text = go.name;

            var view_mold = type.Q<EnumField>("ViewMold");
            view_mold.Init(_viewScriptData?.mold ?? ViewMold.UI2D);
            view_mold.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                var mold = (ViewMold)Enum.Parse(typeof(ViewMold), evt.newValue);
                _viewScriptData.mold = mold;
            });

            var layer = type.Q<IntegerField>("Layer");
            layer.value = _viewScriptData?.layer ?? 0;
            layer.RegisterCallback<ChangeEvent<int>>((evt) => { _viewScriptData.layer = evt.newValue; });

            var active = type.Q<Toggle>("Active");
            active.value = _viewScriptData?.active ?? false;
            active.RegisterCallback<ChangeEvent<bool>>((evt) => { _viewScriptData.active = evt.newValue; });

            var items = (from Transform transform in go.transform select GetTreeViewItemData(transform)).ToList();

            treeView.SetRootItems(items);
            treeView.makeItem = MakeTreeViewItem;
            treeView.bindItem = BindTreeViewItem;
            treeView.selectionChanged += OnItemsChosen;
            treeView.selectionType = SelectionType.Single;
            treeView.Rebuild();
        }

        private TreeViewItemData<ViewData> GetTreeViewItemData(Transform root, string path = "")
        {
            var currentPath = path == string.Empty ? root.name : path + "/" + root.name;
            var childTreeViewItemData =
                (from Transform transform in root select GetTreeViewItemData(transform, currentPath)).ToList();

            var uiViewData = new ViewData { path = currentPath, name = root.name };
            var vd = _viewScriptData.views.FirstOrDefault(d => d.path == currentPath);
            if (vd != null && vd.path == currentPath)
            {
                uiViewData = vd;
            }
            else
            {
                _viewScriptData.views.Add(uiViewData);
            }

            return new TreeViewItemData<ViewData>(root.GetInstanceID(), uiViewData, childTreeViewItemData);
        }

        private VisualElement MakeTreeViewItem()
        {
            var item = new Label { style = { marginTop = 2 } };
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
            if (!enumerable.Any()) return;
            if (enumerable.First() is not ViewData data) return;
            _selectedViewData = data;
            buttons.Q<Label>().text = data.path;
            RefreshView(data);
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        private void RefreshView(ViewData data)
        {
            child.Clear();
            if (data == null) return;
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
        /// <param name="cds"></param>
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
                    name = go.name.TrimAll(),
                    type = component.GetType().Name,
                    fullname = component.GetType().FullName,
                    eventType = GetEventType(component),
                    isPublic = true,
                };
                if (cds != null)
                {
                    var result = cds.Any(cd => data.fullname == cd.fullname);
                    if (result) continue;
                }

                components.Add(data);
            }

            return components;
        }

        private string CreatePublicVariableContent()
        {
            var sb = new StringBuilder();
            foreach (var str in from uiViewData in _viewScriptData.views
                     from t in uiViewData.components
                     where t.isPublic
                     let name = $"{t.name}{t.type}"
                     select $"\t\tpublic {t.type} {name};")
            {
                sb.AppendLine(str);
            }

            return sb.ToString();
        }


        private string CreateRegisterContent()
        {
            var sb = new StringBuilder();
            foreach (var str in _viewScriptData.views.SelectMany(uiViewData => from t in uiViewData.components
                         let str = ""
                         let tName = t.type
                         let name = $"{uiViewData.name}{tName}"
                         select t.eventType switch
                         {
                             1 =>
                                 $"\t\t\t{name}.onClick.AddListener(() => {{ SendEventMsg(\"{name}Event\"); }});",
                             2 =>
                                 $"\t\t\t{name}.onValueChanged.AddListener((arg) => {{ SendEventMsg(\"{name}Event\", arg); }});",
                             3 =>
                                 $"\t\t\t{name}.onValueChanged.AddListener((arg) => {{ SendEventMsg(\"{name}Event\", arg); }});\n" +
                                 $"\t\t\t{name}.onSubmit.AddListener((arg) => {{ SendEventMsg(\"{name}SubmitEvent\", arg); }});\n" +
                                 $"\t\t\t{name}.onSelect.AddListener((arg) => {{ SendEventMsg(\"{name}SelectEvent\", arg); }});\n" +
                                 $"\t\t\t{name}.onDeselect.AddListener((arg) => {{ SendEventMsg(\"{name}DeselectEvent\", arg); }});",
                             _ => str
                         }
                         into str
                         where !string.IsNullOrEmpty(str)
                         select str))
            {
                sb.AppendLine(str);
            }

            return sb.ToString();
        }

        private string CreateInitContent()
        {
            var sb = new StringBuilder();
            foreach (var str in _viewScriptData.views.SelectMany(uiViewData =>
                         from t in uiViewData.components
                         let name = $"{t.name}{t.type}"
                         let path = $"\"{uiViewData.path}\""
                         select $"\t\t\t{name} = this.FindComponent<{t.type}>({path});"))
            {
                sb.AppendLine(str);
            }

            return sb.ToString();
        }

        private string CreateEvent()
        {
            var sb = new StringBuilder();
            foreach (var uiViewData in _viewScriptData.views)
            {
                foreach (var t in uiViewData.components)
                {
                    if (t.eventType == 0) continue;
                    var eventType = GetEventParameterType(t.type);
                    var tName = t.type;
                    var name = $"{uiViewData.name}{tName}";
                    var str = string.IsNullOrEmpty(eventType) ? "" : "arg";
                    sb.AppendLine($"\t\t\tAddEventMsg{eventType}(\"{name}Event\", ({str})=>{{ }});");
                }
            }

            return sb.ToString();
        }

        private string GetEventParameterType(string typeName)
        {
            return typeName switch
            {
                "InputField" or "TMP_InputField" => "<string>",
                "Dropdown" or "TMP_Dropdown" => "<int>",
                "Slider" or "Scrollbar" => "<float>",
                "Toggle" => "<bool>",
                _ => ""
            };
        }

        private int GetEventType(Component component)
        {
            return component switch
            {
                UnityEngine.UI.Button => 1,
                TMPro.TMP_Dropdown or
                    UnityEngine.UI.Toggle or
                    UnityEngine.UI.Dropdown or
                    UnityEngine.UI.Slider => 2,
                UnityEngine.UI.InputField or
                    TMPro.TMP_InputField => 3,
                _ => 0
            };
        }
    }
}