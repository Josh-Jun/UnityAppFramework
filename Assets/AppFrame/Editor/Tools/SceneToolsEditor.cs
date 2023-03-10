using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AppFrame.Editor
{
    [InitializeOnLoad]
    public class SceneToolsEditor
    {
        private static bool Auto;
        private static string key = "SCENE_TOOLS_AUTO";

        static SceneToolsEditor()
        {
            SceneView.duringSceneGui += SceneGUI;
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, 0);
            }
        }

        static Rect windowRect = new Rect(10, 64, 64, 64);

        //sv_iconselector_selection
        static void SceneGUI(SceneView view)
        {
            Handles.BeginGUI();
            windowRect = GUILayout.Window(0, windowRect, DoWindowEvent, "", new GUIStyle("sv_iconselector_selection"));
            Handles.EndGUI();
        }

        static void DoWindowEvent(int windowId)
        {
            Auto = PlayerPrefs.GetInt(key) == 1;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Auto Open Scene"));
            Auto = EditorGUILayout.Toggle(Auto);
            EditorGUILayout.EndHorizontal();
            PlayerPrefs.SetInt(key, Auto ? 1 : 0);

            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Open App Scene"))
            {
                OpenScene();
            }

            if (GUILayout.Button("Play&Pause"))
            {
                EditorApplication.isPlaying = !EditorApplication.isPlaying;
            }

            EditorGUILayout.EndVertical();

            GUI.DragWindow();
        }

        public static void OpenScene()
        {
            string path = "Assets/App/Scenes/Launcher.unity";
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
                    if (Auto)
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