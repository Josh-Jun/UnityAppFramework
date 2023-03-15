using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public static class ChangePrefabsFont
{
    public static string Browse()
    {
        var newPath = EditorUtility.OpenFolderPanel("Prefabs Folder", Application.dataPath, string.Empty);
        if (!string.IsNullOrEmpty(newPath))
        {
            var gamePath = System.IO.Path.GetFullPath(".");
            gamePath = gamePath.Replace("\\", "/");
            if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                newPath = newPath.Remove(0, gamePath.Length + 1);
        }
        return newPath;
    }
    public static void ChangeFont(Font changeFont, string prefabsPath)
    {
        if (changeFont == null)
        {
            return;
        }

        string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { prefabsPath });
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                SetAllTextFont(obj.transform, changeFont);
            }
        }

        AssetDatabase.Refresh();
    }

    private static void SetAllTextFont(Transform go, Font changeFont)
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
                SetAllTextFont(item, changeFont);
            }
        }
    }
}