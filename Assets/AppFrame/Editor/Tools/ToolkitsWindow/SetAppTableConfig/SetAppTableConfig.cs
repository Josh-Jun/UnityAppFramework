using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using AppFrame.Config;
using AppFrame.Enum;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public class SetAppTableConfig : IToolkitEditor
    {
        private AppTableConfig config = null;
        private const string configPath = "AssetsFolder/Table/Config/AppTableConfig";
        private List<string> XmlTableNames = new List<string>();
        private List<string> JsonTableNames = new List<string>();
        
        private string basePath = "Assets/AppFrame/Runtime/Frame/Manager/Table/Data/";
        public void OnCreate(VisualElement root)
        {
            var table_scroll_view = root.Q<ScrollView>("table_scroll_view");
            RefreshTableView(table_scroll_view);
            root.Q<Button>("btn_table_apply").clicked += ApplyConfig;
        }
        private void RefreshTableView(ScrollView table_scroll_view)
        {
            var table_list = GetAppTables();

            if (table_list.Count > 0)
            {
                table_scroll_view.Clear();
                for (int i = 0; i < table_list.Count; i++)
                {
                    int index = i;
                    var tableItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/AppFrame/Editor/Tools/ToolkitsWindow/SetAppTableConfig/table_item.uxml");
                    VisualElement table = tableItem.CloneTree();
                    List<string> tablenames = new List<string>();
                    table.Q<Button>("btn_table_remove").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabDeleted Icon").image);
                    table.Q<Button>("btn_table_add").style.backgroundImage =
                        new StyleBackground((Texture2D)EditorGUIUtility.IconContent("CollabCreate Icon").image);

                    table.Q<EnumField>("TableMold").Init(table_list[index].TableMold);
                    table.Q<EnumField>("TableMold").RegisterCallback<ChangeEvent<string>>((ent) =>
                    {
                        var mold = (TableMold)System.Enum.Parse(typeof(TableMold), ent.newValue);
                        SetConfigMoldValue(index, mold);
                        tablenames = table_list[index].TableMold == TableMold.Json? JsonTableNames : XmlTableNames;
                        table.Q<DropdownField>("TableName").choices = tablenames;
                        if (tablenames.Count > 0)
                        {
                            table.Q<DropdownField>("TableName").value = tablenames[0];
                        }
                    });
                    
                    tablenames = table_list[index].TableMold == TableMold.Json? JsonTableNames : XmlTableNames;
                    table.Q<DropdownField>("TableName").choices = tablenames;
                    table.Q<DropdownField>("TableName").value = tablenames[tablenames.IndexOf(table_list[index].TableName)];
                    table.Q<DropdownField>("TableName").RegisterCallback<ChangeEvent<string>>((ent) =>
                    {
                        SetConfigNameValue(index, ent.newValue);
                    });
                    
                    table.Q<Button>("btn_table_remove").clicked += () =>
                    {
                        RemoveTable(index);
                        RefreshTableView(table_scroll_view);
                    };
                    table.Q<Button>("btn_table_add").clicked += () =>
                    {
                        AddTable();
                        RefreshTableView(table_scroll_view);
                    };
                    table_scroll_view.Add(table);
                }
            }
        }
        private List<AppTable> GetAppTables()
        {
            config = Resources.Load<AppTableConfig>(configPath);

            GetTablePath();
            
            return config.AppTable;
        }

        private void GetTablePath()
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
        
        private void AddTable()
        {
            AppTable appTable = new AppTable
            {
                TableName = "TestTableData",
                TableMold = TableMold.Json,
            };
            config.AppTable.Add(appTable);
        }

        private void RemoveTable(int index)
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

        private void SetConfigNameValue(int index, string value)
        {
            config.AppTable[index].TableName = value;
        }
        private void SetConfigMoldValue(int index, TableMold value)
        {
            config.AppTable[index].TableMold = value;
        }
        
        private void ApplyConfig()
        {
            EditorUtility.SetDirty(config);
            AssetDatabase.Refresh();
        }
    }
}
