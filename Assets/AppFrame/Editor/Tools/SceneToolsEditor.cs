using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AppFrame.Editor
{
    [InitializeOnLoad]
    public class SceneToolsEditor
    {
        private const string AUTO_KEY = "EDITOR_AUTO";
        static SceneToolsEditor()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            if (!PlayerPrefs.HasKey(AUTO_KEY))
            {
                PlayerPrefs.SetInt(AUTO_KEY, 0);
            }
            Menu.SetChecked("App/AutoOpenScene", PlayerPrefs.GetInt(AUTO_KEY) == 1);
        }
        
        [MenuItem("App/AutoOpenScene", false, 10)]
        public static void AutoOpenScene()
        {
            Menu.SetChecked("App/AutoOpenScene", PlayerPrefs.GetInt(AUTO_KEY) != 1);
            var value = Menu.GetChecked("App/AutoOpenScene") ? 1 : 0;
            PlayerPrefs.SetInt(AUTO_KEY, value);
        }

        private static void OpenScene()
        {
            string path = "Assets/AppMain/Scenes/Launcher.unity";
            string SceneName = Path.GetFileNameWithoutExtension(path);
            bool IsCurScene = EditorSceneManager.GetActiveScene().name.Equals(SceneName); //是否为当前场景
            if (!Application.isPlaying)
            {
                if (!IsCurScene)
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode: //停止播放事件监听后被监听
                    // Debug.Log("如果编辑器应用程序处于编辑模式而之前处于播放模式，则在编辑器应用程序的下一次更新期间发生。");
                    break;
                case PlayModeStateChange.ExitingEditMode: //编辑转播放时监听(播放之前)
                    // Debug.Log("在退出编辑模式时，在编辑器处于播放模式之前发生。");
                    if (Menu.GetChecked("App/AutoOpenScene"))
                    {
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