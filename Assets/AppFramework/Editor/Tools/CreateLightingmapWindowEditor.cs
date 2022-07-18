using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CreateLightingmapWindowEditor : EditorWindow
{
    private GUIStyle titleStyle;
    private GameObject target;
	private string outputPath;
    
    [MenuItem("Tools/My ToolsWindow/Create LightingmapData", false, 4)]
    public static void OpenWindow()
    {
        CreateLightingmapWindowEditor window = GetWindow<CreateLightingmapWindowEditor>("CreateLightingmapWindow");
        window.Show();
    }
    private void OnEnable()
    {
        titleStyle = new GUIStyle("OL Title")
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12
        };
        outputPath = "Assets";
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target");
        target = (GameObject)EditorGUILayout.ObjectField(target, typeof(GameObject), true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("OutputPath");
        outputPath = EditorGUILayout.TextField(outputPath);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Save"))
        {
            Save();
        }

    }

    private void Save()
    {
        var lightingmap = ScriptableObject.CreateInstance<LightingmapData>(); 
        AssetDatabase.CreateAsset(lightingmap, @outputPath + "/LightingmapData" + ".asset");//在传入的路径中创建资源
        AssetDatabase.SaveAssets(); //存储资源
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        LightingmapInfo info = null;
        lightingmap.lightingmapInfos = new List<LightingmapInfo>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            info = new LightingmapInfo { lightmapIndex = r.lightmapIndex, lightmapScaleOffset = r.lightmapScaleOffset };
            lightingmap.lightingmapInfos.Add(info);
        }
        EditorUtility.SetDirty(lightingmap);
        AssetDatabase.Refresh(); //刷新
    }
}
