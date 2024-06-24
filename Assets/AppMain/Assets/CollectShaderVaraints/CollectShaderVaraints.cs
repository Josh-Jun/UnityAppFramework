using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CollectShaderVaraints : MonoBehaviour
{
    public MeshRenderer m_MeshRenderer;
    public const int TaskNum = 20;
    public List<string> m_MatPathList = new List<string>();
    public Material[] m_Materials = new Material[TaskNum];

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        FindMaterials();
        yield return new WaitForEndOfFrame();
        
        int sec = Mathf.CeilToInt(m_MatPathList.Count / (float)TaskNum);
        for (int i = 0; i < sec; ++i)
        {
            var baseIndex = i * TaskNum;
            for (int j = 0; j < TaskNum; ++j)
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

    public void FindMaterials()
    {
        string[] filesPath = Directory.GetFiles(Application.dataPath,"*.mat", SearchOption.AllDirectories);
        for (int i = 0; i < filesPath.Length; i++)
        {
            string matPath = filesPath[i].Replace(Application.dataPath, "Assets");
            m_MatPathList.Add(matPath);
        }
    }
}
