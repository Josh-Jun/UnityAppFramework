using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;

namespace App.Core.Tools
{
    /// <summary>
    /// 功能:Excel工具 
    /// </summary>
    public class ExcelTools
    {
        public static void WriteExcel(string path, List<ExcelData> excel)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var fs = new FileStream(path, FileMode.Create);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fs);
            for (var i = 0; i < excel.Count; i++)
            {
                var worksheet = package.Workbook.Worksheets.Add(excel[i].sheetName);
                for (var r = 1; r < excel[i].datas.GetLength(0); r++)
                {
                    for (var c = 1; c < excel[i].datas.GetLength(1); c++)
                    {
                        worksheet.Cells[r, c].Value = excel[i].datas[r,c];
                    }
                }
            }

            package.Save();
        }

        public static List<ExcelData> ReadExcel(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Not Found Excel: {path}");
            }

            using var fs = new FileStream(path, FileMode.Open);
            var excel = new List<ExcelData>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(fs);
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                if(worksheet.Dimension == null) continue;
                var data = new ExcelData
                {
                    sheetName = worksheet.Name,
                    datas = new object[worksheet.Dimension.End.Row + 1,worksheet.Dimension.End.Column + 1]
                };
                for (int c = worksheet.Dimension.Start.Column, c1 = worksheet.Dimension.End.Column; c <= c1; c++)
                {
                    for (int r = worksheet.Dimension.Start.Row, r1 = worksheet.Dimension.End.Row; r <= r1; r++)
                    {
                        data.datas[r,c] = worksheet.GetValue(r, c);
                    }
                }

                excel.Add(data);
            }

            return excel;
        }
    }

    public struct ExcelData
    {
        public string sheetName;
        public object[,] datas;
    }
}