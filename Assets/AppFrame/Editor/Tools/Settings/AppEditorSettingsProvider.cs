/* *
 * ===============================================
 * author      : Josh@book
 * e-mail      : shijun_z@163.com
 * create time : 2024年7月17 10:10
 * function    :
 * ===============================================
 * */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public static class AppEditorSettingsProvider
    {
        [MenuItem("App/Settings", false, 1)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/AppToolkit");
        
        [SettingsProvider]
        public static SettingsProvider Parent()
        {
            var provider = new SettingsProvider("Project/AppToolkit", SettingsScope.Project)
            {
                label = "AppToolkit",
                guiHandler = (text) =>
                {
                    var itemsName = EditorTool.GetScriptName("App.Frame.Editor","IToolkitEditor", false).ToArray();
                    for (int i = 0; i < itemsName.Length; i++)
                    {
                        int index = i;
                        if (GUILayout.Button(itemsName[index]))
                        {
                            SettingsService.OpenProjectSettings($"Project/AppToolkit/{itemsName[index]}");
                        }
                    }
                }
            };
            return  provider;
        }
        
        [SettingsProviderGroup]
        public static SettingsProvider[] Child()
        {
            var provaders = new List<SettingsProvider>();
            var itemsName = EditorTool.GetScriptName("App.Frame.Editor","IToolkitEditor", false).ToArray();
            for (int i = 0; i < itemsName.Length; i++)
            {
                int index = i;
                var provader = new SettingsProvider($"Project/AppToolkit/{itemsName[index]}", SettingsScope.Project)
                {
                    activateHandler = (searchContext, rootElement) =>
                    {
                        var title = new Label()
                        {
                            text = $" {itemsName[index]}",
                            style =
                            {
                                fontSize = 20,
                                unityFontStyleAndWeight = FontStyle.Bold,
                            },
                            
                        };
                        rootElement.Add(title);
                        var viewItem = EditorTool.GetEditorWindowsAsset<VisualTreeAsset>($"{itemsName[index]}/{itemsName[index]}.uxml");
                        var view = viewItem.CloneTree();
                        rootElement.Add(view);
                        
                        var editor = EditorTool.GetEditor(itemsName[index]);
                        editor.OnCreate(rootElement);
                    },
                    inspectorUpdateHandler = () =>
                    {
                        var editor = EditorTool.GetEditor(itemsName[index]);
                        editor.OnUpdate();
                    }
                };
                provaders.Add(provader);
            }
            return provaders.ToArray();
        }
    }
}