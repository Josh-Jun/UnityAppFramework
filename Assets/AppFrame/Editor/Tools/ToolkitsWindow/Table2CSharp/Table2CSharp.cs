using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AppFrame.Enum;
using AppFrame.Tools;
using UnityEditor;
using UnityEngine;

namespace AppFrame.Editor
{
    public class Table2CSharp
    {
        private static List<ExcelData> excelDatas = new List<ExcelData>();

        public static void Apply(string excelPath, TableMold tableMold)
        {
            if (string.IsNullOrEmpty(excelPath))
            {
                ToolkitsWindow.ShowHelpBox("请选择Excel所在目录");
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(excelPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Extension.Equals(".xlsx"))
                {
                    excelDatas = ExcelTools.ReadExcel($"{excelPath}/{fileInfo.Name}");
                    for (int j = 0; j < excelDatas.Count; j++)
                    {
                        ExcelData excel = excelDatas[j];
                        if (excel.sheetName.Contains("#")) continue;
                        switch (tableMold)
                        {
                            case TableMold.Json:
                                CreateJsonCSharp(excel);
                                CreateJsonConfig(excel);
                                break;
                            case TableMold.Xml:
                                //CreateXmlCSharp(excel);
                                break;
                        }
                    }
                }
            }
        }

        private static void CreateJsonCSharp(ExcelData data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("namespace AppFrame.Data");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"    [System.Serializable]");
            stringBuilder.AppendLine($"    public class {data.sheetName}Json");
            stringBuilder.AppendLine("    {");

            stringBuilder.AppendLine($"        public List<{data.sheetName}> {data.sheetName} = new List<{data.sheetName}>();");

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

            string output = string.Format("{0}/AppFrame/Runtime/Frame/Manager/Table/Data/{1}Json.cs", Application.dataPath, data.sheetName);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(stringBuilder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();

            AssetDatabase.Refresh();
        }

        private static void CreateJsonConfig(ExcelData data)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine($"  \"{data.sheetName}\": [");

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
                string _str = r == data.datas.GetLength(0) - 1 ? "" : ",";
                stringBuilder.AppendLine("    }" + _str);
            }

            stringBuilder.AppendLine("  ]");
            
            stringBuilder.Append("}");

            string output = string.Format("{0}/Resources/AssetsFolder/Table/Json/{1}Json.json", Application.dataPath, data.sheetName);
            FileStream fs1 = new FileStream(output, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs1);
            sw.WriteLine(stringBuilder.ToString()); //开始写入值
            sw.Close();
            fs1.Close();

            AssetDatabase.Refresh();
        }

        private static void CreateXmlConfig()
        {
        }
        
        private static void CreateXmlCSharp(ExcelData data)
        {
        }

    }
}