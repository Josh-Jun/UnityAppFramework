using App.Editor.Helper;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace App.Editor.Tools
{
    [InitializeOnLoad]
    public class SceneToolsEditor
    {
        private const int MENU_LEVEL = 0;
        private const string MENU_AUTO_PLAY_PATH = "App/Editor/Launcher/AutoPLay _F8";
        private const string MENU_AUTO_PLAY_VALUE = "MENU_AUTO_PLAY_VALUE";
        private const string TEMP_SCENE = "TEMP_SCENE";
        
        
        private const string MENU_RESTORE_GAME_VIEW_PATH = "App/Editor/Launcher/RestoreGameView _F11";
        private const string MENU_RESTORE_GAME_VIEW_VALUE = "MENU_RESTORE_GAME_VIEW_VALUE";
        static SceneToolsEditor()
        {
            EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
            if (!PlayerPrefs.HasKey(MENU_AUTO_PLAY_VALUE))
            {
                PlayerPrefs.SetInt(MENU_AUTO_PLAY_VALUE, 0);
            }
            Menu.SetChecked(MENU_AUTO_PLAY_PATH, PlayerPrefs.GetInt(MENU_AUTO_PLAY_VALUE) == 1);
            if (!PlayerPrefs.HasKey(MENU_RESTORE_GAME_VIEW_VALUE))
            {
                PlayerPrefs.SetInt(MENU_RESTORE_GAME_VIEW_VALUE, 1);
            }
            Menu.SetChecked(MENU_RESTORE_GAME_VIEW_PATH, PlayerPrefs.GetInt(MENU_RESTORE_GAME_VIEW_VALUE) == 1);
        }
        
        [MenuItem(MENU_AUTO_PLAY_PATH, false, MENU_LEVEL)]
        public static void AutoOpenScene()
        {
            Menu.SetChecked(MENU_AUTO_PLAY_PATH, PlayerPrefs.GetInt(MENU_AUTO_PLAY_VALUE) != 1);
            var value = Menu.GetChecked(MENU_AUTO_PLAY_PATH) ? 1 : 0;
            PlayerPrefs.SetInt(MENU_AUTO_PLAY_VALUE, value);
        }
        [MenuItem(MENU_RESTORE_GAME_VIEW_PATH, false, MENU_LEVEL)]
        public static void RestoreGameView()
        {
            Menu.SetChecked(MENU_RESTORE_GAME_VIEW_PATH, PlayerPrefs.GetInt(MENU_RESTORE_GAME_VIEW_VALUE) != 1);
            var value = Menu.GetChecked(MENU_RESTORE_GAME_VIEW_PATH) ? 1 : 0;
            PlayerPrefs.SetInt(MENU_RESTORE_GAME_VIEW_VALUE, value);
        }

        private static void OpenScene(int index = 0)
        {
            var scene = EditorBuildSettings.scenes[index];
            if(scene == null) return;
            var isCurScene = SceneManager.GetActiveScene().path.Equals(scene.path); //是否为当前场景
            if (Application.isPlaying) return;
            if (!isCurScene)
            {
                EditorSceneManager.OpenScene(scene.path);
            }
        }
        
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
            }
        }
    }
}