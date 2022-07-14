using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class SceneToolsEditor : Editor
{
    void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }
    Rect windowRect = new Rect(10, 10, 64, 64);
    void SceneGUI(SceneView view)
    {
        Handles.BeginGUI();
        windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "Tools");
        Handles.EndGUI();
    }
    void DoMyWindow(int windowID)
    {
        if (GUILayout.Button("Open App Scene"))
        {
            OpenScene();
        }
        if (GUILayout.Button("Play&Pause"))
        {
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
        GUI.DragWindow();
    }
    public static void OpenScene()
    {
        string path = "Assets/AppFramework/Scenes/App.unity";
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
}