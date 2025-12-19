using System;
using System.Collections.Generic;
using System.Reflection;
using App.Editor.Helper;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace App.Editor.Tools
{
    public static class InitializeOnLoadToolsEditor
    {
        private const int MENU_LEVEL = 0;
        private const string MENU_AUTO_PLAY_PATH = "App/Editor/Launcher/AutoPLay _F8";
        private const string TEMP_SCENE = "TEMP_SCENE";


        private const string MENU_RESTORE_GAME_VIEW_PATH = "App/Editor/Launcher/RestoreGameView _F11";

        private const string ToolbarTypeName = "UnityEditor.Toolbar";
        private const string RootFieldName = "m_Root";
        private const string TargetElementName = "ToolbarZoneRightAlign";
        private static Type s_ToolbarType;
        private static ScriptableObject s_CurrentToolbar;
        private static VisualElement s_CustomToolbarParent;
        private static IMGUIContainer s_CustomContainer;
        private static bool s_Initialized = false;
        private static bool s_InitializationFailed = false;

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
            var auto_play_key = GetKey(MENU_AUTO_PLAY_PATH);
            if (!PlayerPrefs.HasKey(auto_play_key))
            {
                PlayerPrefs.SetInt(auto_play_key, 0);
            }
            Menu.SetChecked(MENU_AUTO_PLAY_PATH, PlayerPrefs.GetInt(auto_play_key) == 1);
            
            var restore_game_view_key = GetKey(MENU_RESTORE_GAME_VIEW_PATH);
            if (!PlayerPrefs.HasKey(restore_game_view_key))
            {
                PlayerPrefs.SetInt(restore_game_view_key, 1);
            }

            Menu.SetChecked(MENU_RESTORE_GAME_VIEW_PATH, PlayerPrefs.GetInt(restore_game_view_key) == 1);

            EditorApplication.update += OnUpdate;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        [MenuItem(MENU_AUTO_PLAY_PATH, false, MENU_LEVEL)]
        public static void AutoOpenScene()
        {
            ToolbarToggleItems[MENU_AUTO_PLAY_PATH].isToggleOn = !ToolbarToggleItems[MENU_AUTO_PLAY_PATH].isToggleOn;
        }

        [MenuItem(MENU_RESTORE_GAME_VIEW_PATH, false, MENU_LEVEL)]
        public static void RestoreGameView()
        {
            ToolbarToggleItems[MENU_RESTORE_GAME_VIEW_PATH].isToggleOn = !ToolbarToggleItems[MENU_RESTORE_GAME_VIEW_PATH].isToggleOn;
        }

        private static void OpenScene(int index = 0)
        {
            var scene = EditorBuildSettings.scenes[index];
            if (scene == null) return;
            var isCurScene = SceneManager.GetActiveScene().path.Equals(scene.path); //是否为当前场景
            if (Application.isPlaying) return;
            if (!isCurScene)
            {
                EditorSceneManager.OpenScene(scene.path);
            }
        }
        // [InitializeOnLoadMethod]
        private static void EditorApplication_PlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode: //停止播放事件监听后被监听
                    // Debug.Log("如果编辑器应用程序处于编辑模式而之前处于播放模式，则在编辑器应用程序的下一次更新期间发生。");
                    if (Menu.GetChecked(MENU_AUTO_PLAY_PATH))
                    {
                        if (PlayerPrefs.HasKey(TEMP_SCENE))
                        {
                            EditorSceneManager.OpenScene(PlayerPrefs.GetString(TEMP_SCENE));
                            PlayerPrefs.DeleteKey(TEMP_SCENE);
                        }
                    }

                    if (Menu.GetChecked(MENU_RESTORE_GAME_VIEW_PATH))
                    {
                        EditorHelper.RestoreGameViewResolution();
                    }

                    break;
                case PlayModeStateChange.ExitingEditMode: //编辑转播放时监听(播放之前)
                    // Debug.Log("在退出编辑模式时，在编辑器处于播放模式之前发生。");
                    if (Menu.GetChecked(MENU_AUTO_PLAY_PATH))
                    {
                        if (SceneManager.GetActiveScene().path != string.Empty)
                        {
                            PlayerPrefs.SetString(TEMP_SCENE, SceneManager.GetActiveScene().path);
                            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        }

                        OpenScene();
                    }

                    break;
                case PlayModeStateChange.EnteredPlayMode: //播放时立即监听
                    // Debug.Log("如果编辑器应用程序处于播放模式而之前处于编辑模式，则在编辑器应用程序的下一次更新期间发生。");
                    break;
                case PlayModeStateChange.ExitingPlayMode: //停止播放立即监听
                    // Debug.Log("在退出播放模式时，在编辑器处于编辑模式之前发生。");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }

        private static void OnUpdate()
        {
            if (s_Initialized || s_InitializationFailed)
                return;

            try
            {
                InitializeToolbar();
            }
            catch (Exception e)
            {
                s_InitializationFailed = true;
                Debug.LogError($"Failed to initialize CruToolbar: {e.Message}");
                Cleanup();
            }
        }

        private static void InitializeToolbar()
        {
            // 1. 获取Toolbar类型
            if (s_ToolbarType == null)
            {
                s_ToolbarType = typeof(UnityEditor.Editor).Assembly.GetType(ToolbarTypeName);
                if (s_ToolbarType == null)
                {
                    throw new Exception($"Could not find type: {ToolbarTypeName}");
                }
            }

            // 2. 获取Toolbar实例
            if (s_CurrentToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(s_ToolbarType);
                if (toolbars == null || toolbars.Length == 0)
                {
                    // Toolbar可能还没创建，等待下一帧
                    return;
                }

                s_CurrentToolbar = (ScriptableObject)toolbars[0];
            }

            // 3. 获取root VisualElement
            var rootField = s_ToolbarType.GetField(RootFieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField == null)
            {
                throw new Exception($"Could not find field: {RootFieldName}");
            }

            var rootVisualElement = rootField.GetValue(s_CurrentToolbar) as VisualElement;
            if (rootVisualElement == null)
            {
                throw new Exception($"Could not get root VisualElement");
            }

            // 4. 查找目标容器
            var toolbarZone = rootVisualElement.Q(TargetElementName);
            if (toolbarZone == null)
            {
                throw new Exception($"Could not find element: {TargetElementName}");
            }

            // 5. 创建自定义UI
            s_CustomToolbarParent = new VisualElement()
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginRight = 8 // 添加一些间距，看起来更自然
                }
            };

            s_CustomContainer = new IMGUIContainer(OnGuiBody)
            {
                style =
                {
                    flexGrow = 0,
                    marginLeft = 4 // 按钮之间的间距
                }
            };

            s_CustomToolbarParent.Add(s_CustomContainer);
            toolbarZone.Add(s_CustomToolbarParent);

            s_Initialized = true;
            EditorApplication.update -= OnUpdate;
        }

        private static readonly Dictionary<string, ToolbarToggleItem> ToolbarToggleItems = new ()
        {
            {
                MENU_AUTO_PLAY_PATH,
                new ToolbarToggleItem
                {
                    key = MENU_AUTO_PLAY_PATH,
                    imageOn = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/auto_play_on.png"),
                    imageOff = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/auto_play_off.png"),
                    tooltip = "Auto Play Launcher Scene",
                    isToggleOn = PlayerPrefs.GetInt(GetKey(MENU_AUTO_PLAY_PATH)) == 1,
                }
                
            },
            {
                MENU_RESTORE_GAME_VIEW_PATH,
                new ToolbarToggleItem
                {
                    key = MENU_RESTORE_GAME_VIEW_PATH,
                    imageOn = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/restore_view_on.png"),
                    imageOff = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/restore_view_off.png"),
                    tooltip = "Restore GameView Resolution",
                    isToggleOn = PlayerPrefs.GetInt(GetKey(MENU_RESTORE_GAME_VIEW_PATH)) == 1,
                }
                
            },
        };
        
        private static readonly List<ToolbarButtonItem> ToolbarButtonItems = new ()
        {
            new ToolbarButtonItem
            {
                image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/file_path_refresh.png"),
                tooltip = "Update AssetPath",
                eventHandler = MenuToolsEditor.UpdateAssetPath,
            },
            new ToolbarButtonItem
            {
                image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/asset_package_refresh.png"),
                tooltip = "Update AssetPackage",
                eventHandler = MenuToolsEditor.UpdateAssetPackage,
            },
            new ToolbarButtonItem
            {
                image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/csharp.png"),
                tooltip = "Protobuf2CS",
                eventHandler = MenuToolsEditor.Protobuf2CS,
            },
            new ToolbarButtonItem
            {
                image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/keystore.png"),
                tooltip = "Update Keystore Setting",
                eventHandler = MenuToolsEditor.UpdateKeystore,
            },
            new ToolbarButtonItem
            {
                image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/copy_csharp.png"),
                tooltip = "Copy TemplateScripts",
                eventHandler = MenuToolsEditor.UpdateKeystore,
            },
            new ToolbarButtonItem
            {
                image = AssetDatabase.LoadAssetAtPath<Texture>("Assets/App/Scripts/Frame/Editor/Tools/Images/app_settings.png"),
                tooltip = "Open App Setting",
                eventHandler = AppEditorSettingsProvider.OpenSettings,
            },
            
        };
        
        private static string GetKey(string menuPath)
        {
            return menuPath.Split(' ')[0].Replace("/", "_").ToUpper();
        }

        private static void SetMenuChecked(string menuPath, int value)
        {
            var key = GetKey(menuPath);
            PlayerPrefs.SetInt(key, value);
            Menu.SetChecked(menuPath, value == 1);
        }

        private static void OnGuiBody()
        {
            // 确保即使在异常情况下也不会破坏整个工具栏
            try
            {
                GUILayout.BeginHorizontal();
                
                foreach (var toolbar in ToolbarToggleItems.Values)
                {
                    GUILayout.Space(4);
                    var image = toolbar.isToggleOn ? toolbar.imageOn : toolbar.imageOff;
                    var content = new GUIContent(image, toolbar.tooltip);
                    toolbar.isToggleOn = GUILayout.Toggle(toolbar.isToggleOn, content, EditorStyles.toolbarButton);
                    var value = toolbar.isToggleOn ? 1 : 0;
                    SetMenuChecked(toolbar.key, value);
                }
                GUILayout.Space(16);
                foreach (var toolbar in ToolbarButtonItems)
                {
                    GUILayout.Space(4);
                    var content = new GUIContent(toolbar.image, toolbar.tooltip);
                    if (GUILayout.Button(content, EditorStyles.toolbarButton))
                    {
                        toolbar.eventHandler?.Invoke();
                    }
                }
                
                GUILayout.EndHorizontal();
            }
            catch (Exception e)
            {
                // 记录错误但不会影响其他UI
                Debug.LogError($"Error in CruToolbar GUI: {e.Message}");
            }
        }

        private static void OnBeforeAssemblyReload()
        {
            Cleanup();
        }

        private static void Cleanup()
        {
            // 移除事件监听
            EditorApplication.update -= OnUpdate;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            // 清理UI元素
            if (s_CustomToolbarParent is { parent: not null })
            {
                s_CustomToolbarParent.parent.Remove(s_CustomToolbarParent);
            }

            s_CustomContainer = null;
            s_CustomToolbarParent = null;
            s_CurrentToolbar = null;
            s_ToolbarType = null;

            s_Initialized = false;
        }
    }

    public class ToolbarToggleItem
    {
        public string key;
        public string tooltip;
        public bool isToggleOn;
        public Texture imageOn;
        public Texture imageOff;
    }

    public class ToolbarButtonItem
    {
        public string tooltip;
        public Texture image;
        public Action eventHandler;
    }
}