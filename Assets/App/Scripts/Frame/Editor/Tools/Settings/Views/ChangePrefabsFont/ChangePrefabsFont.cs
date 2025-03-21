using App.Editor.Helper;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace App.Editor.View
{
    public class ChangePrefabsFont : IToolkitEditor
    {
        public void OnCreate(VisualElement root)
        {
            var textPrefabsField = root.Q<TextField>("text_prefab_path");
            textPrefabsField.value = "";
            var fontObjectField = root.Q<ObjectField>("object_font");
            fontObjectField.objectType = typeof(Font);

            root.Q<Button>("prefab_path_browse").clicked += () => { textPrefabsField.value = EditorHelper.Browse(); };

            root.Q<Button>("change_font_apply").clicked += () =>
            {
                ChangeFont((Font)fontObjectField.value, textPrefabsField.value);
            };
        }
        public void OnUpdate()
        {
            
        }
        public void OnDestroy()
        {
            
        }

        private void ChangeFont(Font changeFont, string prefabsPath)
        {
            if (changeFont == null)
            {
                Log.I("请选择要更换的字体");
                return;
            }

            if (string.IsNullOrEmpty(prefabsPath))
            {
                Log.I("请选择要更换的预制体路径");
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
            Log.I("字体更换完成");
        }

        private void SetAllTextFont(Transform go, Font changeFont)
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