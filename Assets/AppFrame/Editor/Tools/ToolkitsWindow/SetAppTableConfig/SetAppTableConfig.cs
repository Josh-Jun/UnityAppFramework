using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public static List<string> XmlTableNames = new List<string>();
        public static List<string> JsonTableNames = new List<string>();
        
        private static string basePath = "Assets/AppFrame/Runtime/Frame/Manager/Table/Data/";
        public static List<AppTable> GetAppTables()
        {
            config = Resources.Load<AppTableConfig>(configPath);

            GetTablePath();
            
            return config.AppTable;
        }

        private static void GetTablePath()
        {
            string[] allPath = AssetDatabase.FindAssets("t:Script", new string[] { basePath });
            for (int i = 0; i < allPath.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(allPath[i]);
                var filename = Path.GetFileNameWithoutExtension(assetPath);
                if (filename.Contains("Xml"))
                {
                    XmlTableNames.Add(filename);
                }
                else if (filename.Contains("Json"))
                {
                    JsonTableNames.Add(filename);
                }
            }
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
