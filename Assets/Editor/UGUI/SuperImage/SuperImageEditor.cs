using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(SuperImage), true)]
public class SuperImageEditor : Editor
{
    private SerializedProperty sprite;
    private SerializedProperty color;
    private SerializedProperty material;
    private SerializedProperty raycastTarget;
    private SerializedProperty segements;
    private SerializedProperty round;
    void OnEnable()
    {
        sprite = serializedObject.FindProperty("m_Sprite");
        color = serializedObject.FindProperty("m_Color");
        material = serializedObject.FindProperty("m_Material");
        raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        segements = serializedObject.FindProperty("segements");
        round = serializedObject.FindProperty("round");
    }

    [MenuItem("GameObject/UI/SuperImage")]
    static void SuperImage()
    {
        GameObject parent = Selection.activeGameObject;
        RectTransform parentCanvasRenderer = parent?.GetComponent<RectTransform>();
        if (parentCanvasRenderer)
        {
            GameObject go = new GameObject("SuperImage");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            go.AddComponent<SuperImage>();
            Selection.activeGameObject = go;
        }
        else
        {
            EditorUtility.DisplayDialog("SuperImage", "You must make the SuperImage object as a child of a Canvas.", "Ok");
        }
    }
    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sprite);
        EditorGUILayout.PropertyField(color);
        EditorGUILayout.PropertyField(material);
        EditorGUILayout.PropertyField(raycastTarget);

        EditorGUILayout.PropertyField(round);
        EditorGUILayout.PropertyField(segements);

        serializedObject.ApplyModifiedProperties();
    }
}
