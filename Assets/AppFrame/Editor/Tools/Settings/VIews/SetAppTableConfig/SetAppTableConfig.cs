using System.Collections.Generic;
using System.IO;
using System.Text;
using AppFrame.Config;
using AppFrame.Enum;
using AppFrame.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppFrame.Editor
{
    public class TableData
    {
        public string tableName;
        public TableMold tableMold;
        public ExcelData excelData;
    }
    public class SetAppTableConfig : IToolkitEditor
    {
        private AppTableConfig config = null;
        private const string configPath = "AssetsFolder/Table/Config/AppTableConfig";
        
        private string excelPath = $"{Application.dataPath.Replace("Assets", "")}Data/excel";
        
        private List<TableData> tableDatas = new List<TableData>();

        private ScrollView table_scroll_view;

        public void OnCreate(VisualElement root)
        {
            table_scroll_view = root.Q<ScrollView>("table_scroll_view");
            config = Resources.Load<AppTableConfig>(configPath);
            
            root.Q<Button>("btn_table_apply").clicked += ApplyConfig;
            InitTableView();
        }

        public void OnUpdate()
        {
            
        }
        public void OnDestroy()
        {
            
        }

        private void InitTableView()
        {
            tableDatas = GetTableDatas();
            table_scroll_view.Clear();
            for (int i = 0; i < tableDatas.Count; i++)
            {
                int index = i;
                var tableEnumField = new EnumField(tableDatas[index].tableMold){ label = $"{tableDatas[index].tableName}" };
                tableEnumField.RegisterCallback<ChangeEvent<string>>((ent) =>
                {
                    var mold = (TableMold)System.Enum.Parse(typeof(TableMold), ent.newValue);
                    tableDatas[index].tableMold = mold;
                });
                table_scroll_view.Add(tableEnumField);
            }
        }

        private List<TableData> GetTableDatas()
        {
            List<TableData> datas = new List<TableData>();
            DirectoryInfo directoryInfo = new DirectoryInfo(excelPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Extension.Equals(".xlsx"))
                {
                    var excelDatas = ExcelTools.ReadExcel($"{excelPath}/{fileInfo.Name}");
                    for (int j = 0; j < excelDatas.Count; j++)
                    {
                        ExcelData excel = excelDatas[j];
                        if (excel.sheetName.Contains("#")) continue;
                        TableData tableData = new TableData()
                        {
                            tableName = excel.sheetName,
                            tableMold = TableMold.Json,
                            excelData = excel
                        };
                        datas.Add(tableData);
                    }
                }
            }
            return datas;
        }
        
        private void ApplyConfig()
        {
            config.AppTable.Clear();
            for (int i = 0; i < tableDatas.Count; i++)
            {
                var data = tableDatas[i];
                switch (data.tableMold)
                {
                    case TableMold.Json:
                        CreateJsonCSharp(data.excelData);
                        CreateJsonConfig(data.excelData);
                        break;
                    case TableMold.Xml:
                        CreateXmlCSharp(data.excelData);
                        CreateXmlConfig(data.excelData);
                        break;
                }

                var appTable = new AppTable()
                {
                    TableName = $"{data.tableName}{data.tableMold}Data",
                    TableMold = data.tableMold
                };
                config.AppTable.Add(appTable);
            }
            EditorUtility.SetDirty(config);
            AssetDatabase.Refresh();
        }
        
        private void CreateJsonCSharp(ExcelData data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("namespace AppFrame.Data.Json");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"    [System.Serializable]");
            stringBuilder.AppendLine($"    public class {data.sheetName}JsonData");
            stringBuilder.AppendLine("    {");

            stringBuilder.AppendLine($"        public List<{data.sheetName}> {data.sheetName}s = new List<{data.sheetName}>();");

            stringBuilder.AppendLine("    }");

            stringBuilder.AppendLine($"    [System.Serializable]");
            stringBuilder.AppendLine($"    public class {data.sheetName}");
            stringBuilder.AppendLine("    {");

            for (int c = 2; c < data.datas.GetLength(1); c++)
            {
                stringBuilder.AppendLine($"        public {data.datas[3,c]} {data.datas[2,c]};");
            }

            stringBuilder.AppendLine("    }");
            stringBuilder.Append("}");

            string output = string.Format("{0}/AppFrame/Runtime/Frame/Manager/Table/Data/{1}JsonData.cs", Application.dataPath, data.sheetName);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(stringBuilder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();
        }

        private void CreateJsonConfig(ExcelData data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"  \"{data.sheetName}s\": [");

            for (int r = 4; r < data.datas.GetLength(0); r++)
            {
                stringBuilder.AppendLine("    {");
                for (int c = 2; c < data.datas.GetLength(1); c++)
                {
                    string str = c == data.datas.GetLength(1) - 1 ? "" : ",";
                    if (data.datas[3, c].ToString() == "string")
                    {
                        stringBuilder.AppendLine($"      \"{data.datas[2,c]}\": \"{data.datas[r,c]}\"{str}");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"      \"{data.datas[2,c]}\": {data.datas[r,c]}{str}");
                    }
                }
                string _str = r == data.datas.GetLength(0) - 1 ? "    }" : "    },";
                stringBuilder.AppendLine(_str);
            }

            stringBuilder.AppendLine("  ]");
            
            stringBuilder.Append("}");

            string output = string.Format("{0}/Resources/AssetsFolder/Table/Json/{1}JsonData.json", Application.dataPath, data.sheetName);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(stringBuilder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();
        }

        private void CreateXmlConfig(ExcelData data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            stringBuilder.AppendLine($"<{data.sheetName}XmlData>");

            for (int r = 4; r < data.datas.GetLength(0); r++)
            {
                stringBuilder.Append($"    <{data.sheetName}s");
                for (int c = 2; c < data.datas.GetLength(1); c++)
                {
                    stringBuilder.Append($" {data.datas[2,c]}=\"{data.datas[r,c]}\"");
                }
                stringBuilder.AppendLine($"></{data.sheetName}s>");
            }

            stringBuilder.Append($"</{data.sheetName}XmlData>");

            string output = string.Format("{0}/Resources/AssetsFolder/Table/Xml/{1}XmlData.xml", Application.dataPath, data.sheetName);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(stringBuilder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();
        }
        
        private void CreateXmlCSharp(ExcelData data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("using System.Xml.Serialization;");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("namespace AppFrame.Data.Xml");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"    [System.Serializable]");
            stringBuilder.AppendLine($"    public class {data.sheetName}XmlData");
            stringBuilder.AppendLine("    {");

            stringBuilder.AppendLine($"        [XmlElement(\"{data.sheetName}\")]");
            stringBuilder.AppendLine($"        public List<{data.sheetName}> {data.sheetName}s = new List<{data.sheetName}>();");

            stringBuilder.AppendLine("    }");

            stringBuilder.AppendLine($"    [System.Serializable]");
            stringBuilder.AppendLine($"    public class {data.sheetName}");
            stringBuilder.AppendLine("    {");

            for (int c = 2; c < data.datas.GetLength(1); c++)
            {
                stringBuilder.AppendLine($"        [XmlAttribute(\"{data.datas[2,c]}\")]");
                stringBuilder.AppendLine($"        public {data.datas[3,c]} {data.datas[2,c]};");
            }

            stringBuilder.AppendLine("    }");
            stringBuilder.Append("}");

            string output = string.Format("{0}/AppFrame/Runtime/Frame/Manager/Table/Data/{1}XmlData.cs", Application.dataPath, data.sheetName);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(stringBuilder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();
        }
    }
}
