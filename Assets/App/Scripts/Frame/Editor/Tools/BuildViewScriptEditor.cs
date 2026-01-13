/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年12月24 15:41
 * function    :
 * ===============================================
 * */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using App.Core.Helper;
using App.Core.Master;
using App.Editor.Helper;
using UnityEngine;
using UnityEditor;

namespace App.Editor.Tools
{
    public static class BuildViewScriptEditor
    {
        private static readonly Dictionary<GUIContent, GenericMenu.MenuFunction2> LayerGenericMenus = new()
        {
            { new GUIContent("0"), obj => { SetViewLayer(0); } },
            { new GUIContent("1"), obj => { SetViewLayer(1); } },
            { new GUIContent("2"), obj => { SetViewLayer(2); } },
            { new GUIContent("3"), obj => { SetViewLayer(3); } },
            { new GUIContent("4"), obj => { SetViewLayer(4); } },
            { new GUIContent("5"), obj => { SetViewLayer(5); } },
            { new GUIContent("6"), obj => { SetViewLayer(6); } },
            { new GUIContent("7"), obj => { SetViewLayer(7); } },
        };
        private static readonly Dictionary<GUIContent, GenericMenu.MenuFunction2> MoldGenericMenus = new()
        {
            { new GUIContent("UI2D"), obj => { SetViewMold(ViewMold.UI2D); } },
            { new GUIContent("UI3D"), obj => { SetViewMold(ViewMold.UI3D); } },
            { new GUIContent("Go3D"), obj => { SetViewMold(ViewMold.Go3D); } },
        };
        private const float MinWindowWidth = 360; // 设置显示图标和Toggle的最小窗口宽度
        private static readonly List<Type> ViewTypes = EditorHelper.GetAssemblyTypes<ViewBase>();
        private static readonly ViewOfAttribute ViewAttribute = new("", ViewMold.UI2D, "");
        private static readonly string scriptViewPath = $"{EditorHelper.BaseEditorPath()}/Tools/LogicViewScript/ui_view_script.txt";
        private static readonly string scriptLogicPath = $"{EditorHelper.BaseEditorPath()}/Tools/LogicViewScript/ui_logic_script.txt";

        private const string prefix = "LV_";
        
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            // 注册 hierarchyWindowItemOnGUI 的回调函数
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static void OnHierarchyGUI(int instanceID, Rect rect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (!gameObject) return;
            if (!gameObject.name.EndsWith("View")) return;
            if (gameObject.name == "UpdateView") return;
            if (GetHierarchyWindowWidth() < MinWindowWidth) return;
            var type = ViewTypes.FirstOrDefault(v => v.Name == gameObject.name);
            var obj = type?.GetCustomAttributes(typeof(ViewOfAttribute), false).FirstOrDefault();
            var isDisabled = true;
            if (obj is not ViewOfAttribute attribute)
            {
                if (!Selection.activeGameObject) return;
                if (Selection.activeGameObject != gameObject) return;
                ViewAttribute.SetName(gameObject.name.Replace("View", ""));
                ViewAttribute.SetLocation($"AssetPath.{gameObject.name}");
                attribute = ViewAttribute;
                isDisabled = false;
            }

            DrawViewActiveToggle(rect, attribute.Active, isDisabled);
            DrawViewLayer(rect, attribute.Layer, isDisabled);
            DrawViewMold(rect, attribute.View, isDisabled);
        }
        
        private static void DrawViewActiveToggle(Rect rect, bool active, bool isDisabled = true)
        {
            var r = new Rect(rect);
            r.x += r.width - 20;
            r.width = 20;
            EditorGUI.BeginDisabledGroup(isDisabled);
            var toggle = GUI.Toggle(r, active, string.Empty);
            if(!isDisabled) ViewAttribute.SetActive(toggle);
            EditorGUI.EndDisabledGroup();
        }
        
        private static void DrawViewLayer(Rect rect, int layer, bool isDisabled = true)
        {
            var r = new Rect(rect);
            r.x += r.width - 80;
            r.width = 64;
            
            if (!isDisabled)
            {
                OnHierarchyGUIGenericMenu(r, LayerGenericMenus);
            }
            
            EditorGUI.BeginDisabledGroup(isDisabled);
            var style = new GUIStyle(EditorStyles.label) { normal = new GUIStyleState() { textColor = Color.yellow } };
            GUI.Label(r, $"层级:[{layer}]", style);
            EditorGUI.EndDisabledGroup();
        }
        
        private static void DrawViewMold(Rect rect, ViewMold viewMold, bool isDisabled = true)
        {
            var r = new Rect(rect);
            r.x += r.width - 160;
            r.width = 72;
            
            if (!isDisabled)
            {
                OnHierarchyGUIGenericMenu(r, MoldGenericMenus);
            }

            EditorGUI.BeginDisabledGroup(isDisabled);
            var style = new GUIStyle(EditorStyles.label) { normal = new GUIStyleState() { textColor = Color.green } };
            GUI.Label(r, $"类型:[{viewMold}]", style);
            EditorGUI.EndDisabledGroup();
        }

        private static void OnHierarchyGUIGenericMenu(Rect rect, Dictionary<GUIContent, GenericMenu.MenuFunction2>  menus)
        {
            if (Event.current == null || !rect.Contains(Event.current.mousePosition) || Event.current.button != 1 || Event.current.type > EventType.MouseUp) return;
            if (!Selection.activeGameObject) return;
            // 创建 GenericMenu
            var menu = new GenericMenu();
            foreach (var item in menus)
            {
                menu.AddItem(item.Key, false, item.Value, Selection.activeGameObject);
            }
            // 在鼠标位置显示菜单
            menu.ShowAsContext();
            Event.current.Use();
        }

        private static void SetViewLayer(int layer)
        {
            ViewAttribute.SetLayer(layer);
        }
        
        private static void SetViewMold(ViewMold viewMold)
        {
            ViewAttribute.SetViewMold(viewMold);
        }
        
        /// <summary>
        /// 获取 Hierarchy 窗口的宽度
        /// </summary>
        /// <returns></returns>
        private static float GetHierarchyWindowWidth()
        {
            var hierarchyInfo = typeof(UnityEditor.Editor).Assembly
                .GetType("UnityEditor.SceneHierarchyWindow")
                ?.GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            var hierarchyWindow = (EditorWindow)hierarchyInfo?.GetValue(null);
            return hierarchyWindow?.position.width ?? 0;
        }
        
        public static void BuildViewScript(GameObject gameObject)
        {
            var view_path = Path.Combine(Application.dataPath, scriptViewPath);
            var view_script = File.ReadAllText(view_path);
            var view_script_name = gameObject.name;
            var folder_name = gameObject.name.Replace("View", "");
            var script_path = Path.Combine(Application.dataPath, $"Scripts/{folder_name}");
            var view_script_path = Path.Combine(script_path, $"{view_script_name}.cs");
            view_script = view_script.Replace("#VIEWMOLD#", $"{ViewAttribute.View}");
            view_script = view_script.Replace("#ACTIVE#", $"{ViewAttribute.Active}".ToLower());
            view_script = view_script.Replace("#LAYER#", $"{ViewAttribute.Layer}");
            view_script = view_script.Replace("#MODULE#", folder_name);
            view_script = view_script.Replace("#SCRIPTNAME#", view_script_name);
            view_script = view_script.Replace("#VARIABLE#", CreatePublicVariableContent());
            view_script = view_script.Replace("#INIT#", CreateInitContent());
            view_script = view_script.Replace("#REGISTER#", CreateRegisterContent());
            view_script = view_script.Replace("#OPEN#", "");
            view_script = view_script.Replace("#CLOSE#", "");
            
            var files = EditorHelper.GetFiles(Path.Combine(Application.dataPath, $"Scripts"), "cs");
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
            AssetDatabase.Refresh();
        }
        
        public static void BuildLogicScript(GameObject gameObject)
        {
            var logic_path = Path.Combine(Application.dataPath, scriptLogicPath);
            var logic_script = File.ReadAllText(logic_path);
            var folder_name = gameObject.name.Replace("View", "");
            var script_path = Path.Combine(Application.dataPath, $"Scripts/{folder_name}");
            var logic_script_name = $"{folder_name}Logic";
            var logic_script_path = Path.Combine(script_path, $"{logic_script_name}.cs");
            
            logic_script = logic_script.Replace("#MODULE#", folder_name);
            logic_script = logic_script.Replace("#SCRIPTNAME#", logic_script_name);
            logic_script = logic_script.Replace("#EVENT#", CreateEvent());
            
            var files = EditorHelper.GetFiles(Path.Combine(Application.dataPath, $"Scripts"), "cs");
            var logic = files.FirstOrDefault(f => f.Name == $"{logic_script_name}.cs");
            if (logic != null) logic_script_path = logic.FullName;
            
            File.WriteAllText(logic_script_path, logic_script);
            AssetDatabase.Refresh();
        }
        
        #region MenuItem
        private const int MenuItemPriority = -1;
        // 为创建自定义对象添加一个菜单。
        // 优先级为10确保它与其他同类菜单项在一组，并传播到hierarchy 下拉菜单和hierarchy 上下文菜单。
        [MenuItem("GameObject/Build View Script", false, MenuItemPriority)]
        public static void BuildViewScript()
        {
            views = GetViewScriptData(Selection.activeGameObject);
            BuildViewScript(Selection.activeGameObject);
        }
        
        // 为创建自定义对象添加一个菜单。
        // 优先级为10确保它与其他同类菜单项在一组，并传播到hierarchy 下拉菜单和hierarchy 上下文菜单。
        [MenuItem("GameObject/Build View&&Logic Script", false, MenuItemPriority)]
        public static void BuildLogicViewScript()
        {
            BuildViewScript();
            BuildLogicScript(Selection.activeGameObject);
        }
        
        [MenuItem("GameObject/Build View&&Logic Script", true)]
        [MenuItem("GameObject/Build View Script", true)]
        public static bool ValidateMenuItemBuildViewScript()
        {
            return Selection.activeGameObject && Selection.activeGameObject.name.EndsWith("View") && Selection.activeGameObject.name != "UpdateView";
        }
        #endregion
        
        #region Private Method
        
        public static List<ViewData> views = new List<ViewData>();
        
        private static List<ViewData> GetViewScriptData(GameObject go)
        {
            var list = new List<ViewData>();
            var queue = new Queue<(Transform Parent, string Path)>();
            // 从根节点开始
            foreach (Transform child in go.transform)
            {
                queue.Enqueue((child, child.name));
                if (!child.name.StartsWith(prefix)) continue;
                var data = new ViewData
                {
                    path = child.name, 
                    name = child.name.Substring(prefix.Length, child.name.Length - prefix.Length),
                    components = GetComponents(child.gameObject)
                };
                list.Add(data);
            }
            while (queue.Count > 0)
            {
                var (parent, path) = queue.Dequeue();
                
                // 将子物体加入队列
                foreach (Transform child in parent)
                {
                    var childPath = path + "/" + child.name;
                    queue.Enqueue((child, childPath));
                    if (!child.name.StartsWith(prefix)) continue;
                    var data = new ViewData
                    {
                        path = childPath, 
                        name = child.name.Substring(prefix.Length, child.name.Length - prefix.Length),
                        components = GetComponents(child.gameObject)
                    };
                    list.Add(data);
                }
            }
            return list;
        }
        
        /// <summary>
        /// 获取UI组件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cds"></param>
        /// <returns></returns>
        private static List<ComponentData> GetComponents(GameObject go, List<ComponentData> cds = null)
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

        private static string CreatePublicVariableContent()
        {
            var sb = new StringBuilder();
            foreach (var str in from uiViewData in views
                     from t in uiViewData.components
                     where t.isPublic
                     let name = $"{uiViewData.name}{t.type}"
                     select $"\t\tpublic {t.type} {name};")
            {
                sb.AppendLine(str);
            }

            return sb.ToString();
        }


        private static string CreateRegisterContent()
        {
            var sb = new StringBuilder();
            foreach (var str in views.SelectMany(uiViewData => from t in uiViewData.components
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

        private static string CreateInitContent()
        {
            var sb = new StringBuilder();
            foreach (var str in views.SelectMany(uiViewData =>
                         from t in uiViewData.components
                         let name = $"{uiViewData.name}{t.type}"
                         let path = $"\"{uiViewData.path}\""
                         select $"\t\t\t{name} = this.FindComponent<{t.type}>({path});"))
            {
                sb.AppendLine(str);
            }

            return sb.ToString();
        }

        private static string CreateEvent()
        {
            var sb = new StringBuilder();
            foreach (var uiViewData in views)
            {
                foreach (var t in uiViewData.components)
                {
                    if (t.eventType == 0) continue;
                    var eventType = GetEventParameterType(t.type);
                    var tName = t.type;
                    var name = $"{uiViewData.name}{tName}";
                    var str = string.IsNullOrEmpty(eventType) ? "" : "arg";
                    sb.AppendLine($"\t\t\tAddEventMsg{eventType}(\"{name}Event\", ({str})=>{{ }});");
                    if (t.eventType != 3) continue;
                    sb.AppendLine($"\t\t\tAddEventMsg{eventType}(\"{name}SubmitEvent\", ({str})=>{{ }});");
                    sb.AppendLine($"\t\t\tAddEventMsg{eventType}(\"{name}SelectEvent\", ({str})=>{{ }});");
                    sb.AppendLine($"\t\t\tAddEventMsg{eventType}(\"{name}DeselectEvent\", ({str})=>{{ }});");
                }
            }

            return sb.ToString();
        }

        private static string GetEventParameterType(string typeName)
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

        private static int GetEventType(Component component)
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
            public string type;
            public string fullname;
            public int eventType;
            public bool isPublic;
        }
        #endregion
    }
}