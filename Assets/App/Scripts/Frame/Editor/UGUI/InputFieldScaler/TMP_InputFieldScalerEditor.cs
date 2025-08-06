/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2025年6月11 13:33
 * function    :
 * ===============================================
 * */
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(TMP_InputFieldScaler))]
    public class TMP_InputFieldScalerEditor : UnityEditor.Editor
    {
        private TMP_InputFieldScaler InputFieldScaler;
        private SerializedObject _target;

        private SerializedProperty fontSize;
        private SerializedProperty fixedWidth;
        private SerializedProperty maxHeight;

        private SerializedProperty keepInitWidthSize;

        // Use this for initialization
        void OnEnable()
        {
            InputFieldScaler = (TMP_InputFieldScaler)target;
            _target = new SerializedObject(target);

            fontSize = _target.FindProperty("fontSize");
            maxHeight = _target.FindProperty("maxHeight");
            fixedWidth = _target.FindProperty("fixedWidth");
            keepInitWidthSize = _target.FindProperty("keepInitWidthSize");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Undo.RecordObject(InputFieldScaler, "object change");

            EditorGUILayout.PropertyField(fontSize);
            EditorGUILayout.PropertyField(maxHeight);
            EditorGUILayout.PropertyField(fixedWidth);
            if (!InputFieldScaler.fixedWidth)
            {
                EditorGUILayout.PropertyField(keepInitWidthSize);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }

            _target.ApplyModifiedProperties();
        }
    }
}