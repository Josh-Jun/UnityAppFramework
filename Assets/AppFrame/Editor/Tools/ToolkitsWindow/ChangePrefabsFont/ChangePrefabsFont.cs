using AppFrame.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AppFrame.Editor
{
    public class ChangePrefabsFont
    {

        public static void ChangeFont(Font changeFont, string prefabsPath)
        {
            if (changeFont == null)
            {
                ToolkitsWindow.ShowHelpBox("请选择要更换的字体");
                return;
            }

            if (string.IsNullOrEmpty(prefabsPath))
            {
                ToolkitsWindow.ShowHelpBox("请选择要更换的预制体路径");
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
            ToolkitsWindow.ShowHelpBox("字体更换完成");
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
}