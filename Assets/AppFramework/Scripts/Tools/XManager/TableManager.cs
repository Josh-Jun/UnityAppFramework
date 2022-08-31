using System;
using System.Collections;
using System.Collections.Generic;
using ThriftTable;
using UnityEngine;

public class TableManager : SingletonMono<TableManager>
{
    private Dictionary<string, Table> m_TablePairs = new Dictionary<string, Table>();
    private AppTableConfig appTableConfig;
    public override void InitManager(Transform parent)
    {
        base.InitManager(parent);
    }
    public void InitConfig()
    {
        appTableConfig = AssetsManager.Instance.LoadAsset<AppTableConfig>(AssetsPathConfig.AppTableConfig);
        for (int i = 0; i < appTableConfig.AppTable.Count; i++)
        {
            var path = $"Table/{appTableConfig.AppTable[i].TableMold}/{appTableConfig.AppTable[i].TableName}";
            var text = AssetsManager.Instance.LoadAsset<TextAsset>(path).text;
            var bytes = AssetsManager.Instance.LoadAsset<TextAsset>(path).bytes;
            Table table = new Table(text, bytes, path, appTableConfig.AppTable[i].TableMold);
            if (m_TablePairs.ContainsKey(appTableConfig.AppTable[i].TableName))
            {
                m_TablePairs[appTableConfig.AppTable[i].TableName] = table;
            }
            else
            {
                m_TablePairs.Add(appTableConfig.AppTable[i].TableName, table);
            }
        }
        for (var en = ThriftTableHolder.ETXT_NAME._Min + 1; en < ThriftTableHolder.ETXT_NAME._Max; en++)
        {
            if (m_TablePairs.ContainsKey(en.ToString()))
            {
                ThriftTableHolder.m_TableDic[en].ParseData(m_TablePairs[en.ToString()].bytes);
            }
        }
    }
    public T GetTable<T>(string tableName) where T : class
    {
        var table = m_TablePairs[tableName];
        T t = null;
        switch (table.mold)
        {
            case TableMold.Xml:
                t = XmlManager.ProtoDeSerialize<T>(table.bytes);
                break;
            case TableMold.Json:
                t = LitJson.JsonMapper.ToObject<T>(table.text);
                break;
            case TableMold.Thrift:
                ThriftTableHolder.ETXT_NAME en = (ThriftTableHolder.ETXT_NAME)Enum.Parse(typeof(ThriftTableHolder.ETXT_NAME), tableName);
                t = ThriftTableHolder.GetTable(en) as T;
                break;
            
        }
        return t;
    }
}

public class Table
{
    public string text;
    public byte[] bytes;
    public string path;
    public TableMold mold;

    public Table(string text, byte[] bytes, string path, TableMold mold)
    {
        this.text = text;
        this.bytes = bytes;
        this.path = path;
        this.mold = mold;
    }
}
