using System.Collections;
using System.Collections.Generic;
using System.IO;
using AppFrame.Enum;
using AppFrame.Tools;
using UnityEngine;

namespace AppFrame.Editor
{
    public class Table2CSharp
    {
        private static List<ExcelPackageData> excelPackageDatas = new List<ExcelPackageData>();
        public static void Apply(string excelPath, TableMold tableMold)
        {
            var fileNames = new List<string>();
            DirectoryInfo directoryInfo = new DirectoryInfo(excelPath);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Extension.Equals(".xlsx"))
                {
                    fileNames.Add(fileInfo.Name);
                }
            }

            for (int i = 0; i < fileNames.Count; i++)
            {
                excelPackageDatas = ExcelTools.ReadExcel($"{excelPath}/{fileNames[i]}");
                for (int j = 0; j < excelPackageDatas.Count; j++)
                {
                    ExcelPackageData excel = excelPackageDatas[j];
                    if(excel.sheetName.Contains("#")) continue;
                    
                }
            }
        }

        private static void CreateCSharp()
        {
            
        }
        private static void CreateConfig()
        {
            
        }
    }
}
