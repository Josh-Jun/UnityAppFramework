using System;
using System.Collections;
using System.Collections.Generic;
using AppFrame.Config;
using AppFrame.Data;
using AppFrame.Enum;
using AppFrame.Tools;
using AppFramework.Data;
using Table.Data;
using UnityEngine;

namespace AppFrame.Manager
{
    public class TableManager : SingletonMono<TableManager>
    {
        private Dictionary<string, Table> m_TablePairs = new Dictionary<string, Table>();
        private AppTableConfig appTableConfig;

        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
            InitConfig();
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
        }

        public T GetTable<T>(string tableName) where T : class
        {
            var table = m_TablePairs[tableName];
            T t = null;
            switch (table.mold)
            {
                case TableMold.Xml:
                    t = XmlTools.ProtoDeSerialize<T>(table.bytes);
                    break;
                case TableMold.Json:
                    t = JsonUtility.FromJson<T>(table.text);
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
}
