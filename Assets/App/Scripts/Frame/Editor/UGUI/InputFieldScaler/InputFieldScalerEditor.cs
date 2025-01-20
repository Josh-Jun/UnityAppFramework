using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(InputFieldScaler))]
    public class InputFieldScalerEditor : UnityEditor.Editor
    {
        private InputFieldScaler InputFieldScaler;
        private SerializedObject _target;

        private SerializedProperty fontSize;
        private SerializedProperty fixedWidth;

        private SerializedProperty keepInitWidthSize;

        // Use this for initialization
        void OnEnable()
        {
            InputFieldScaler = (InputFieldScaler)target;
            _target = new SerializedObject(target);

            fontSize = _target.FindProperty("fontSize");
            fixedWidth = _target.FindProperty("fixedWidth");
            keepInitWidthSize = _target.FindProperty("keepInitWidthSize");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Undo.RecordObject(InputFieldScaler, "object change");

            EditorGUILayout.PropertyField(fontSize);
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
