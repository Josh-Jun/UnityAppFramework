using System.Collections.Generic;
using AppFrame.Config;
using AppFrame.Enum;
using AppFrame.Info;
using AppFrame.Tools;
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
            if (appTableConfig == null)
            {
                appTableConfig = AssetsManager.Instance.LoadAsset<AppTableConfig>(AppInfo.AssetPathPairs[nameof(AppTableConfig)]);
                for (int i = 0; i < appTableConfig.AppTable.Count; i++)
                {
                    var path = $"Table/{appTableConfig.AppTable[i].TableMold}/{appTableConfig.AppTable[i].TableName}";
                    var textAsset = AssetsManager.Instance.LoadAsset<TextAsset>(path);
                    Table table = new Table(textAsset.text, textAsset.bytes, path, appTableConfig.AppTable[i].TableMold);
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
