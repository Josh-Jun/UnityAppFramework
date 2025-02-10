using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CollectShaderVaraints : MonoBehaviour
{
    public MeshRenderer m_MeshRenderer;
    private const int TaskNum = 20;
    public List<string> m_MatPathList = new List<string>();
    public Material[] m_Materials = new Material[TaskNum];

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        FindMaterials();
        yield return new WaitForEndOfFrame();
        
        var sec = Mathf.CeilToInt(m_MatPathList.Count / (float)TaskNum);
        for (var i = 0; i < sec; ++i)
        {
            var baseIndex = i * TaskNum;
            for (var j = 0; j < TaskNum; ++j)
            {
                var index = baseIndex + j;
                if (index < m_MatPathList.Count)
                {
#if UNITY_EDITOR
                    m_Materials[j] = AssetDatabase.LoadAssetAtPath<Material>(m_MatPathList[index]);
#endif
                }
            }
            m_MeshRenderer.materials = m_Materials;
            yield return new WaitForEndOfFrame();
        }
    }

    private void FindMaterials()
    {
        var filesPath = Directory.GetFiles(Application.dataPath,"*.mat", SearchOption.AllDirectories);
        foreach (var t in filesPath)
        {
            var matPath = t.Replace(Application.dataPath, "Assets");
            m_MatPathList.Add(matPath);
        }
    }
}
