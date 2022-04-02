using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class ChangePrefabsFontWindowEditor : EditorWindow
{

    //private static Texture texture;
    private static GUIStyle titleStyle;

    #region 1、更换预制体字体
    private string prefabsPath = "";

    private Font changeFont;

    #endregion

    [MenuItem("Tools/My ToolsWindow/ChangePrefabsFont", false, 2)]
    public static void OpenWindow()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };

        ChangePrefabsFontWindowEditor window = GetWindow<ChangePrefabsFontWindowEditor>("Change Prefabs Font");
        window.Show();
    }

    private void OnGUI()
    {
        #region 1、更换预制体字体
        GUILayout.BeginVertical();

        changeFont = (Font)EditorGUILayout.ObjectField(changeFont, typeof(Font), true);

        EditorGUILayout.Space();

        prefabsPath = EditorGUILayout.TextField("Prefabs Path", prefabsPath);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
        {
            var newPath = EditorUtility.OpenFolderPanel("Prefabs Folder", prefabsPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                var gamePath = System.IO.Path.GetFullPath(".");
                gamePath = gamePath.Replace("\\", "/");
                if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                    newPath = newPath.Remove(0, gamePath.Length + 1);
                prefabsPath = newPath;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Change Prefabs Font(更换字体)"))
        {
            EditorApplication.delayCall += ChangePrefabsFont;
        }

        GUILayout.EndVertical();
        #endregion
    }

    #region 1、更换预制体字体
    private void ChangePrefabsFont()
    {
        if (changeFont == null)
        {
            Debug.Log("请选择要更换的字体");
            return;
        }
        string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { prefabsPath });
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                SetAllTextFont(obj.transform);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("更换完成");
    }
    private void SetAllTextFont(Transform go)
    {
        foreach (Transform item in go)
        {
            Text t = item.GetComponent<Text>();
            if (t != null)
            {
                t.font = changeFont;
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
            }
            if (item.childCount > 0)
            {
                SetAllTextFont(item);
            }
        }
    }
    #endregion

}
