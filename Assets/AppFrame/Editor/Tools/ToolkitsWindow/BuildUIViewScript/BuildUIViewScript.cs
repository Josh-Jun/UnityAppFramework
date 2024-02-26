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
        public string path;
        public Component component;
    }

    public class BuildUIViewScript : IToolkitEditor
    {
        private VisualTreeAsset uiGameObject;
        private Foldout rootFoldout;
        private ObjectField uiGameObjectField;
        private Dictionary<string, GameObject> allPaths = new Dictionary<string, GameObject>();


        private List<UIViewData> uiViewDatas = new List<UIViewData>();

        private string templetPath = $"{EditorTool.BasePath}/BuildUIViewScript/ui_view_script_templet.txt";

        private string viewScriptPath = "AppMain/Scripts/Modules/";

        public void OnCreate(VisualElement root)
        {
            //获取item
            uiGameObject = EditorTool.GetEditorAsset<VisualTreeAsset>($"BuildUIViewScript/view_item.uxml");
            //获取根节点Foldout，并隐藏
            rootFoldout = root.Q<Foldout>("UIView");
            rootFoldout.style.display = DisplayStyle.None;

            //获取对象ObjectField，并进行初始化
            uiGameObjectField = root.Q<ObjectField>("UIGameObject");
            uiGameObjectField.objectType = typeof(GameObject);
            uiGameObjectField.RegisterCallback<ChangeEvent<Object>>((evt) =>
            {
                if (evt.newValue != null)
                {
                    OnOnjectFieldChange(evt.newValue as GameObject);
                }
                else
                {
                    rootFoldout.style.display = DisplayStyle.None;
                    rootFoldout.Clear();
                }
            });

            //获取按钮，并添加按钮事件
            root.Q<Button>("BtnBuild").clicked += BuildEvent;
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
            //清除数据设置根节点Foldout的名称，并显示
            uiViewDatas.Clear();
            rootFoldout.text = go.name;
            rootFoldout.style.display = DisplayStyle.Flex;
            //清除所有对象路径数据
            allPaths.Clear();
            //获取所有对象数据
            GetUIChildren(go.transform);
            //添加首条item
            Add();
            //刷新界面
            RefreshView();
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        private void RefreshView()
        {
            if (uiViewDatas.Count > 0)
            {
                rootFoldout.Clear();
                for (int i = 0; i < uiViewDatas.Count; i++)
                {
                    int index = i;
                    VisualElement item = uiGameObject.CloneTree();
                    //删除和添加按钮设置图标和添加事件
                    item.Q<Button>("btn_remove").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabDeleted Icon").image);
                    item.Q<Button>("btn_add").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabCreate Icon").image);

                    item.Q<Button>("btn_remove").clicked += () =>
                    {
                        Remove(index);
                        RefreshView();
                    };
                    item.Q<Button>("btn_add").clicked += () =>
                    {
                        Add();
                        RefreshView();
                    };
                    //设置itemid
                    item.Q<Label>("Count").text = index.ToString();
                    //设置路径下拉框
                    item.Q<DropdownField>("UIPath").choices = allPaths.Keys.ToList();
                    item.Q<DropdownField>("UIPath").value = uiViewDatas[index].path;
                    item.Q<DropdownField>("UIPath").Focus();
                    item.Q<DropdownField>("UIPath").RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        uiViewDatas[index].path = evt.newValue;
                        RefreshView();
                    });

                    //获取当前对象上的所有组件
                    GameObject parent = allPaths[uiViewDatas[index].path];
                    var components = GetUIComponents(parent);
                    //将组件转换成下拉框的选项
                    var names = new List<string>();
                    for (var j = 0; j < components.Length; j++)
                    {
                        names.Add(components[j].GetType().FullName);
                    }

                    //设置组件下拉框的值，并添加相应事件
                    var dropdownField = item.Q<DropdownField>("UIComponent");
                    dropdownField.choices = names;
                    if (components.Contains(uiViewDatas[index].component))
                    {
                        dropdownField.value = names[names.IndexOf(uiViewDatas[index].component.GetType().FullName)];
                    }
                    else
                    {
                        uiViewDatas[index].component = components[0];
                        dropdownField.value = names[0];
                    }

                    dropdownField.RegisterCallback<ChangeEvent<string>>((evt) =>
                    {
                        uiViewDatas[index].component = components[names.IndexOf(evt.newValue)];
                    });

                    rootFoldout.Add(item);
                }
            }
        }

        /// <summary>
        /// 添加一条item
        /// </summary>
        private void Add()
        {
            var data = new UIViewData();
            data.path = allPaths.First().Key;
            uiViewDatas.Add(data);
        }

        /// <summary>
        /// 删除一条item
        /// </summary>
        /// <param name="index"></param>
        private void Remove(int index)
        {
            if (uiViewDatas.Count > 1)
            {
                uiViewDatas.RemoveAt(index);
            }
            else
            {
                ToolkitsWindow.ShowHelpBox("不能删除最后一个Item");
            }
        }

        ///  <summary>
        /// 获取所有UI对象
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        private void GetUIChildren(Transform parent, string path = "")
        {
            foreach (Transform child in parent)
            {
                // 构建当前子物体的完整路径
                string currentPath = path == string.Empty ? child.name : path + "/" + child.name;
                if (!allPaths.ContainsKey(currentPath))
                {
                    allPaths.Add(currentPath, child.gameObject);
                }
                else
                {
                    ToolkitsWindow.ShowHelpBox("同级路径下不能重名，请检查:" + currentPath);
                }

                // 递归调用，检查子物体的子对象
                GetUIChildren(child, currentPath);
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
            for (int i = 0; i < uiViewDatas.Count; i++)
            {
                var _str_ = i == 0 ? "" : "\t\t";
                var type = uiViewDatas[i].component.GetType().Name;
                var var_name = uiViewDatas[i].path.Split('/').Last().ToLower();
                string str = $"{_str_}private {type} {var_name};";
                sb.AppendLine(str);
            }

            return sb.ToString();
        }

        private string CraetePublicVariableContent()
        {
            StringBuilder sb = new StringBuilder();
            bool isfirst = true;
            for (int i = 0; i < uiViewDatas.Count; i++)
            {
                if (IsPublic(uiViewDatas[i].component))
                {
                    var _str_ = isfirst ? "" : "\t\t";
                    isfirst = false;
                    var type = uiViewDatas[i].component.GetType().Name;
                    var var_name = uiViewDatas[i].path.Split('/').Last().ToLower();
                    var var_name1 = ToUpperCase(var_name);
                    string str = $"{_str_}public {type} {var_name1} {{ get {{ return {var_name}; }} set {{ {var_name} = value; }}}}";
                    sb.AppendLine(str);
                }
            }

            return sb.ToString();
        }


        private string CraeteRegisterContent()
        {
            StringBuilder sb = new StringBuilder();
            bool isfirst = true;
            for (int i = 0; i < uiViewDatas.Count; i++)
            {
                string str = "";
                var _str_ = isfirst ? "" : "\t\t\t";
                var var_name = uiViewDatas[i].path.Split('/').Last().ToLower();
                
                if (IsEvent(uiViewDatas[i].component))
                {
                    isfirst = false;
                    if (uiViewDatas[i].component is UnityEngine.UI.Button)
                    {
                        str = $"{_str_}{var_name}.onClick.AddListener(() => {{ SendEventMsg(\"{ToUpperCase(var_name)}Event\"); }});";
                    }
                    else
                    {
                        str = $"{_str_}{var_name}.onValueChanged.AddListener((arg) => {{ SendEventMsg(\"{ToUpperCase(var_name)}Event\", arg); }});";
                    }
                }
                
                if (!string.IsNullOrEmpty(str))
                {
                    sb.AppendLine(str);
                }
            }

            return sb.ToString();
        }

        private string CraeteInitContent()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < uiViewDatas.Count; i++)
            {
                var _str_ = i == 0 ? "" : "\t\t\t";
                var var_name = uiViewDatas[i].path.Split('/').Last().ToLower();
                var type = uiViewDatas[i].component.GetType().Name;
                var path = $"\"{uiViewDatas[i].path}\"";
                string str = $"{_str_}{var_name} = this.FindComponent<{type}>({path});";
                sb.AppendLine(str);
            }

            return sb.ToString();
        }

        private string ToUpperCase(string str)
        {
            var _str = "";
            for (int i = 0; i < str.Length; i++)
            {
                var s = str[i].ToString();
                s = s.ToLower();
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