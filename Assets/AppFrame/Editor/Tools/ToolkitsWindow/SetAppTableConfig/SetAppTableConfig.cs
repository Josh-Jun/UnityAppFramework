using System.Collections;
using System.Collections.Generic;
using AppFrame.Config;
using AppFrame.Enum;
using UnityEditor;
using UnityEngine;

namespace AppFrame.Editor
{
    public class SetAppTableConfig
    {
        private static AppTableConfig config = null;
        private const string configPath = "AssetsFolder/Table/Config/AppTableConfig";
        
        public static List<AppTable> GetAppTables()
        {
            if (config == null)
            {
                config = Resources.Load<AppTableConfig>(configPath);
            }
            return config.AppTable;
        }
        
        public static void AddTable()
        {
            AppTable appTable = new AppTable
            {
                TableName = "TestTableData",
                TableMold = TableMold.Json,
            };
            config.AppTable.Add(appTable);
        }

        public static void RemoveTable(int index)
        {
            if (config.AppTable.Count > 1)
            {
                config.AppTable.RemoveAt(index);
            }
            else
            {
                ToolkitsWindow.ShowHelpBox("不能删除最后一个Table");
            }
        }

        public static void SetConfigNameValue(int index, string value)
        {
            config.AppTable[index].TableName = value;
        }
        public static void SetConfigMoldValue(int index, TableMold value)
        {
            config.AppTable[index].TableMold = value;
        }
        
        public static void ApplyConfig()
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.Refresh();
        }
    }
}
