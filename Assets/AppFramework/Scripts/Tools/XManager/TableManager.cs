using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : SingletonMono<TableManager>
{
    private const string path_AppTableConfig = "App/Assets/AppTableConfig";
    private Dictionary<string, byte[]> m_TablePairs = new Dictionary<string, byte[]>();
    private AppTableConfig appTableConfig;
    public override void InitManager(Transform parent)
    {
        base.InitManager(parent);
    }
    public void InitConfig()
    {
        TextAsset config = AssetsManager.Instance.LoadAsset<TextAsset>(path_AppTableConfig);
        appTableConfig = XmlManager.ProtoDeSerialize<AppTableConfig>(config.bytes);
        for (int i = 0; i < appTableConfig.AppTable.Count; i++)
        {
            var bytes = AssetsManager.Instance.LoadAsset<TextAsset>(appTableConfig.AppTable[i].TablePath).bytes;
            if (m_TablePairs.ContainsKey(appTableConfig.AppTable[i].TableName))
            {
                m_TablePairs[appTableConfig.AppTable[i].TableName] = bytes;
            }
            else
            {
                m_TablePairs.Add(appTableConfig.AppTable[i].TableName, bytes);
            }
        }
    }
    public T GetTable<T>(string tableName) where T : class
    {
        return XmlManager.ProtoDeSerialize<T>(m_TablePairs[tableName]);
    }
}
