using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

namespace App.Editor.Helper
{
    /// <summary>
    /// 功能:Excel工具 
    /// </summary>
    public static class ExcelHelper
    {
        public static void WriteExcel(string path, List<ExcelData> excel)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var fs = new FileStream(path, FileMode.Create);
            using var package = new ExcelPackage(fs);
            for (var i = 0; i < excel.Count; i++)
            {
                var worksheet = package.Workbook.Worksheets.Add(excel[i].sheetName);
                for (var r = 1; r < excel[i].datas.GetLength(0); r++)
                {
                    for (var c = 1; c < excel[i].datas.GetLength(1); c++)
                    {
                        worksheet.Cells[r, c].Value = excel[i].datas[r, c];
                    }
                }
            }

            package.Save();
        }

        public static List<ExcelData> ReadExcel(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Not Found Excel: {filePath}");
            }

            using var fs = new FileStream(filePath, FileMode.Open);
            var excel = new List<ExcelData>();
            using var package = new ExcelPackage(fs);
            Parallel.ForEach(package.Workbook.Worksheets, worksheet =>
            {
                var data = new ExcelData
                {
                    sheetName = worksheet.Name,
                    datas = new object[worksheet.Dimension.End.Row + 1, worksheet.Dimension.End.Column + 1]
                };
                lock (excel)
                {
                    for (int c = worksheet.Dimension.Start.Column, c1 = worksheet.Dimension.End.Column; c <= c1; c++)
                    {
                        for (int r = worksheet.Dimension.Start.Row, r1 = worksheet.Dimension.End.Row; r <= r1; r++)
                        {
                            data.datas[r, c] = worksheet.GetValue(r, c);
                        }
                    }

                    excel.Add(data);
                }
            });

            return excel;
        }
    }

    public struct ExcelData
    {
        public string sheetName;
        public object[,] datas;
    }
}