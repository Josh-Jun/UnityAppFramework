using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public class UIViewData
    {
        public string name;
        public string path;
        public GameObject go;
        public List<Component> components = new List<Component>();
    }

    public class BuildUIViewScript : IToolkitEditor
    {
        private VisualTreeAsset view_item;
        private ObjectField uiGameObjectField;
        private Dictionary<string, UIViewData> allUIObjects = new Dictionary<string, UIViewData>();

        private string templetPath = $"{EditorTool.BaseWindowPath}/BuildUIViewScript/ui_view_script_templet.txt";

        private string viewScriptPath = "AppMain/Scripts/Modules/";

        private VisualElement parent;
        private VisualElement child;
        private VisualElement btns;
        private Foldout rootFoldout;
        private TreeView treeView;
        
        private DropdownField pathField;
        private DropdownField uiComponentField;

        private UIViewData selectedUIViewData;

        public void OnCreate(VisualElement root)
        {
            //获取item
            view_item = EditorTool.GetEditorWindowsAsset<VisualTreeAsset>($"BuildUIViewScript/view_item.uxml");
            //获取根节点Foldout
            rootFoldout = root.Q<Foldout>("UIView");
            
            parent = root.Q<VisualElement>("Root");
            child = root.Q<VisualElement>("Child");
            btns = root.Q<VisualElement>("Btns");
            parent.style.display = DisplayStyle.None;
            btns.style.display = DisplayStyle.None;

            //获取对象ObjectField，并进行初始化
            uiGameObjectField = root.Q<ObjectField>("UIGameObject");
            uiGameObjectField.objectType = typeof(GameObject);
            uiGameObjectField.RegisterCallback<ChangeEvent<Object>>((evt) =>
            {
                OnOnjectFieldChange(evt.newValue as GameObject);
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
                if (selectedUIViewData != null)
                {
                    selectedUIViewData.components.RemoveAt(selectedUIViewData.components.Count - 1);
                    RefreshView(selectedUIViewData);
                }
            };
            root.Q<Button>("btn_add").clicked += () =>
            {
                if (selectedUIViewData != null)
                {
                    var components = GetUIComponents(selectedUIViewData.go);
                    selectedUIViewData.components.Add(components[0]);
                    RefreshView(selectedUIViewData);
                }
            };
        }

        private void BuildEvent()
        {
            var path = Path.Combine(Application.dataPath, templetPath);
            var script = File.ReadAllText(path);

            var script_name = rootFoldout.text;
            var folder_name = rootFoldout.text.Replace("View", "");
            var script_path = Path.Combine(Application.dataPath, $"{viewScriptPath}/{folder_name}");
            viewScriptPath = Path.Combine(script_path, $"{script_name}.cs");

            script = script.Replace("#NAMESPACE#", folder_name);
            script = script.Replace("#SCRIPTNAME#", script_name);
            script = script.Replace("#VARIABLE#", CraetePrivateVariableContent());
            script = script.Replace("#GETSET#", CraetePublicVariableContent());
            script = script.Replace("#INIT#", CraeteInitContent());
            script = script.Replace("#REGISTER#", CraeteRegisterContent());
            script = script.Replace("#OPEN#", "");
            script = script.Replace("#CLOSE#", "");
            if (!Directory.Exists(script_path))
            {
                Directory.CreateDirectory(script_path);
            }

            if (File.Exists(viewScriptPath))
            {
                File.Delete(viewScriptPath);
            }

            File.WriteAllText(viewScriptPath, script);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 当对象选择有变化时调用
        /// </summary>
        /// <param name="go"></param>
        private void OnOnjectFieldChange(GameObject go)
        {
            parent.style.display = go ? DisplayStyle.Flex : DisplayStyle.None;
            btns.style.display = go ? DisplayStyle.Flex : DisplayStyle.None;
            if(go == null) return;
            rootFoldout.text = go.name;
            
            var items = new List<TreeViewItemData<UIViewData>>();

            foreach (Transform child in go.transform)
            {
                var treeViewItemData = GetTreeViewItemData(child);
                items.Add(treeViewItemData);
            }
            
            treeView.SetRootItems(items);
            treeView.makeItem = MakeTreeViewItem;
            treeView.bindItem = BindTreeViewItem;
            treeView.selectionChanged += OnItemsChosen;
            treeView.selectionType = SelectionType.Single;
            treeView.Rebuild();
        }

        private TreeViewItemData<UIViewData> GetTreeViewItemData(Transform parent, string path = "")
        {
            var childTreeViewItemDatas = new List<TreeViewItemData<UIViewData>>();
            string currentPath = path == string.Empty ? parent.name : path + "/" + parent.name;
            foreach (Transform child in parent)
            {
                var itemData = GetTreeViewItemData(child, currentPath);
                childTreeViewItemDatas.Add(itemData);
            }
            
            var uiViewData = new UIViewData { path = currentPath, name = parent.name, go = parent.gameObject };
            if (!allUIObjects.ContainsKey(currentPath))
            {
                allUIObjects.Add(currentPath, uiViewData);
            }
            else
            {
                ToolkitsWindow.ShowHelpBox("同级路径下不能重名，请检查:" + currentPath);
            }
            return new TreeViewItemData<UIViewData>(parent.GetInstanceID(), uiViewData, childTreeViewItemDatas);
        }

        private VisualElement MakeTreeViewItem()
        {
            var item = new Label { style = { marginTop = 2 }};
            return item;
        }
        private void BindTreeViewItem(VisualElement ve, int index)
        {
            var item = ve as Label;
            var data = treeView.GetItemDataForIndex<UIViewData>(index);
            if (item != null)
            {
                item.text = data.name;
            }
        }
        private void OnItemsChosen(IEnumerable<object> objs)
        {
            var data = objs.First() as UIViewData;
            if (data != null)
            {
                selectedUIViewData = data;
                Debug.Log(data.path);
                RefreshView(data);
            }
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        private void RefreshView(UIViewData data)
        {
            child.Clear();
            for (int i = 0; i < data.components.Count; i++)
            {
                int index = i;
                VisualElement item = view_item.CloneTree();
                //设置itemid
                item.Q<Label>("Count").text = index.ToString();
                //获取当前对象上的所有组件
                var components = GetUIComponents(data.go);
                //将组件转换成下拉框的选项
                var names = new List<string>();
                for (var j = 0; j < components.Length; j++)
                {
                    names.Add(components[j].GetType().FullName);
                }
                
                //设置组件下拉框的值，并添加相应事件
                var dropdownField = item.Q<DropdownField>("UIComponent");
                dropdownField.choices = names;
                if (components.Contains(data.components[index]))
                {
                    dropdownField.value = names[names.IndexOf(data.components[index].GetType().FullName)];
                }
                else
                {
                    data.components[index] = components[0];
                    dropdownField.value = names[0];
                }
                
                dropdownField.RegisterCallback<ChangeEvent<string>>((evt) =>
                {
                    data.components[index] = components[names.IndexOf(evt.newValue)];
                });
                
                child.Add(item);
            }
        }

        /// <summary>
        /// 获取UI组件
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private Component[] GetUIComponents(GameObject go)
        {
            var uiComponents = new List<Component>();
            var components = go.GetComponents<Component>();
            foreach (var component in components)
            {
                //剔除CanvasRenderer
                if (component is CanvasRenderer)
                {
                    continue;
                }

                uiComponents.Add(component);
            }

            return uiComponents.ToArray();
        }

        private string CraetePrivateVariableContent()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var uiViewData in allUIObjects)
            {
                for (int i = 0; i < uiViewData.Value.components.Count; i++)
                {
                    var type = uiViewData.Value.components[i].GetType().Name;
                    var name = $"{uiViewData.Value.name.ToLower()}{type}";
                    string str = $"\t\tprivate {type} {name};";
                    sb.AppendLine(str);
                }
            }
        
            return sb.ToString();
        }
        
        private string CraetePublicVariableContent()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var uiViewData in allUIObjects)
            {
                for (int i = 0; i < uiViewData.Value.components.Count; i++)
                {
                    var type = uiViewData.Value.components[i].GetType().Name;
                    var name = $"{uiViewData.Value.name.ToLower()}{type}";
                    var nameUpperCase = $"{ToUpperCase(name)}";
                    string str = $"\t\tpublic {type} {nameUpperCase} {{ get {{ return {name}; }} set {{ {name} = value; }}}}";
                    sb.AppendLine(str);
                }
            }
            return sb.ToString();
        }
        
        
        private string CraeteRegisterContent()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var uiViewData in allUIObjects)
            {
                for (int i = 0; i < uiViewData.Value.components.Count; i++)
                {
                    string str = "";
                    var type = uiViewData.Value.components[i].GetType().Name;
                    var name = $"{uiViewData.Value.name.ToLower()}{type}";
                    if (IsEvent(uiViewData.Value.components[i]))
                    {
                        if (uiViewData.Value.components[i] is UnityEngine.UI.Button)
                        {
                            str = $"\t\t\t{name}.onClick.AddListener(() => {{ SendEventMsg(\"{ToUpperCase(name)}Event\"); }});";
                        }
                        else
                        {
                            str = $"\t\t\t{name}.onValueChanged.AddListener((arg) => {{ SendEventMsg(\"{ToUpperCase(name)}Event\", arg); }});";
                        }
                    }
                    if (!string.IsNullOrEmpty(str))
                    {
                        sb.AppendLine(str);
                    }
                }
            }
        
            return sb.ToString();
        }
        
        private string CraeteInitContent()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var uiViewData in allUIObjects)
            {
                for (int i = 0; i < uiViewData.Value.components.Count; i++)
                {
                    var type = uiViewData.Value.components[i].GetType().Name;
                    var name = $"{uiViewData.Value.name.ToLower()}{type}";
                    var path = $"\"{uiViewData.Value.path}\"";
                    string str = $"\t\t\t{name} = this.FindComponent<{type}>({path});";
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
            if (component is UnityEngine.UI.Text ||
                component is UnityEngine.UI.InputField ||
                component is UnityEngine.UI.Dropdown ||
                component is UnityEngine.UI.Slider ||
                component is UnityEngine.UI.Toggle ||
                component is UnityEngine.UI.Image ||
                component is UnityEngine.UI.RawImage ||
                component is UnityEngine.UI.GridLayoutGroup ||
                component is UnityEngine.UI.HorizontalLayoutGroup ||
                component is UnityEngine.UI.VerticalLayoutGroup ||
                component is UnityEngine.UI.ContentSizeFitter ||
                component is UnityEngine.CanvasGroup ||
                component is UnityEngine.Canvas)
            {
                return true;
            }

            return false;
        }

        private bool IsEvent(Component component)
        {
            if (component is UnityEngine.UI.Button ||
                component is UnityEngine.UI.InputField ||
                component is UnityEngine.UI.Dropdown ||
                component is UnityEngine.UI.Slider ||
                component is UnityEngine.UI.Toggle ||
                component is UnityEngine.UI.ScrollRect ||
                component is UnityEngine.UI.Scrollbar)
            {
                return true;
            }

            return false;
        }
    }
}