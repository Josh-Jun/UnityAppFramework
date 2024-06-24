using UnityEngine.UI;

namespace UnityEditor.UI
{

    [CustomEditor(typeof(ScrollViewEx))]
    public class ScrollViewExEditor : ScrollViewEditor
    {
        SerializedProperty pageSize;


        protected override void OnEnable()
        {
            base.OnEnable();
            pageSize = serializedObject.FindProperty("m_pageSize");
        }

        protected override void DrawConfigInfo()
        {
            base.DrawConfigInfo();
            EditorGUILayout.PropertyField(pageSize);
        }
        
        [MenuItem("GameObject/UI/ScrollViewEx", false, 90)]
        static public void AddScrollViewEx(MenuCommand menuCommand)
        {
            InternalAddScrollView<ScrollViewEx>(menuCommand);
        }
    }
}