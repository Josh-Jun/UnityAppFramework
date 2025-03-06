/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年7月17 10:10
 * function    :
 * ===============================================
 * */

using System.Collections.Generic;
using System.Linq;
using App.Editor.Helper;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace App.Editor.Tools
{
    public static class AppEditorSettingsProvider
    {
        [MenuItem("App/Settings _F6", false, 1)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/AppToolkit");
        
        [SettingsProvider]
        public static SettingsProvider Parent()
        {
            var provider = new SettingsProvider("Project/AppToolkit", SettingsScope.Project)
            {
                titleBarGuiHandler = () =>
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    var buttonStyle = GUI.skin.GetStyle("IconButton");

                    #region  绘制官方网站跳转按钮
                    var w = rect.x + rect.width;
                    rect.x = w - 19;
                    rect.y += 6;
                    rect.width = rect.height = 18;
                    var content = EditorGUIUtility.IconContent("_Help");
                    content.tooltip = "点击访问 GitHub 查看源码";
                    if (GUI.Button(rect, content, buttonStyle))
                    {
                        Application.OpenURL("https://github.com/Josh-Jun/UnityAppFramework");
                    }
                    #endregion

                },
                guiHandler = (text) =>
                {
                    var itemsName = EditorHelper.GetScriptName("App.Editor","IToolkitEditor", false).ToArray();
                    for (var i = 0; i < itemsName.Length; i++)
                    {
                        var index = i;
                        var paths = itemsName[index].Split('_');
                        var path = paths.Aggregate("", (current, p) => current + $"/{p}");
                        if (GUILayout.Button(paths[^1]))
                        {
                            SettingsService.OpenProjectSettings($"Project/AppToolkit{path}");
                        }
                    }
                }
            };
            return  provider;
        }
        private static readonly List<string> childs = new List<string>();
        [SettingsProviderGroup]
        public static SettingsProvider[] Child()
        {
            childs.Clear();
            var provaders = new List<SettingsProvider>();
            var itemsName = EditorHelper.GetScriptName("App.Editor","IToolkitEditor", false).ToArray();
            for (var i = 0; i < itemsName.Length; i++)
            {
                var index = i;
                var paths = itemsName[index].Split('_');
                var path = "";
                for (var j = 0; j < paths.Length; j++)
                {
                    path += $"/{paths[j]}";
                    if (j >= paths.Length - 1) continue;
                    if (childs.Contains($"Project/AppToolkit{path}")) continue;
                    var provader_null =
                        new SettingsProvider($"Project/AppToolkit{path}", SettingsScope.Project);
                    childs.Add($"Project/AppToolkit{path}");
                    provaders.Add(provader_null);
                }
                var provader = new SettingsProvider($"Project/AppToolkit{path}", SettingsScope.Project)
                {
                    activateHandler = (searchContext, rootElement) =>
                    {
                        var title = new Label()
                        {
                            text = $" {paths[^1]}",
                            style =
                            {
                                fontSize = 20,
                                unityFontStyleAndWeight = FontStyle.Bold,
                            },
                            
                        };
                        rootElement.Add(title);
                        var viewItem = EditorHelper.GetEditorWindowsAsset<VisualTreeAsset>($"{paths[^1]}/{paths[^1]}.uxml");
                        var view = viewItem.CloneTree();
                        rootElement.Add(view);
                        
                        var editor = EditorHelper.GetEditor(itemsName[index]);
                        editor.OnCreate(rootElement);
                    },
                    inspectorUpdateHandler = () =>
                    {
                        var editor = EditorHelper.GetEditor(itemsName[index]);
                        editor.OnUpdate();
                    },
                    deactivateHandler = () =>
                    {
                        var editor = EditorHelper.GetEditor(itemsName[index]);
                        editor.OnDestroy();
                    }
                };
                provaders.Add(provader);
            }
            return provaders.ToArray();

        }
    }
}