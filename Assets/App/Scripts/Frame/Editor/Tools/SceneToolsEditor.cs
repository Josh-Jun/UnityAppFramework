using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace App.Editor.Tools
{
    [InitializeOnLoad]
    public class SceneToolsEditor
    {
        private const string MENU_PATH = "App/Launcher/AutoLoad";
        private const string MENU_VALUE = "MENU_VALUE";
        private const string TEMP_SCENE = "TEMP_SCENE";
        static SceneToolsEditor()
        {
            EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
            if (!PlayerPrefs.HasKey(MENU_VALUE))
            {
                PlayerPrefs.SetInt(MENU_VALUE, 0);
            }
            Menu.SetChecked(MENU_PATH, PlayerPrefs.GetInt(MENU_VALUE) == 1);
        }
        
        [MenuItem(MENU_PATH, false, 10)]
        public static void AutoOpenScene()
        {
            Menu.SetChecked(MENU_PATH, PlayerPrefs.GetInt(MENU_VALUE) != 1);
            var value = Menu.GetChecked(MENU_PATH) ? 1 : 0;
            PlayerPrefs.SetInt(MENU_VALUE, value);
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
                    if (Menu.GetChecked(MENU_PATH))
                    {
                        if (PlayerPrefs.HasKey(TEMP_SCENE))
                        {
                            EditorSceneManager.OpenScene(PlayerPrefs.GetString(TEMP_SCENE));
                            PlayerPrefs.DeleteKey(TEMP_SCENE);
                        }
                    }
                    break;
                case PlayModeStateChange.ExitingEditMode: //编辑转播放时监听(播放之前)
                    // Debug.Log("在退出编辑模式时，在编辑器处于播放模式之前发生。");
                    if (Menu.GetChecked(MENU_PATH))
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