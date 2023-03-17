using System.Collections;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;

namespace AppFrame.Tools
{
    public class ExcelTools
    {
        public static void WriteExcel(string path, List<ExcelPackageData> excelPackages)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            FileStream fs = new FileStream(path, FileMode.Create);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(fs))
            {
                for (int i = 0; i < excelPackages.Count; i++)
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(excelPackages[i].sheetName);
                    for (int j = 0; j < excelPackages[i].datas.Count; j++)
                    {
                        ExcelData data = excelPackages[i].datas[j];
                        worksheet.Cells[data.axes.row, data.axes.col].Value = data.data.ToString();
                    }
                }

                package.Save();
            }
        }

        public static List<ExcelPackageData> ReadExcel(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"Not Found Excel: {path}");
            }

            using FileStream fs = new FileStream(path, FileMode.Open);
            List<ExcelPackageData> excelPackages = new List<ExcelPackageData>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new ExcelPackage(fs);
            for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[i];
                if(worksheet.Dimension == null) continue;
                ExcelPackageData data = new ExcelPackageData();
                data.sheetName = worksheet.Name;
                data.datas = new List<ExcelData>();
                for (int j = worksheet.Dimension.Start.Column, k = worksheet.Dimension.End.Column; j <= k; j++)
                {
                    for (int m = worksheet.Dimension.Start.Row, n = worksheet.Dimension.End.Row; m <= n; m++)
                    {
                        ExcelData excelData = new ExcelData();
                        excelData.axes = new ExcelAxes(m, j);
                        excelData.data = worksheet.GetValue(m, j);
                        data.datas.Add(excelData);
                    }
                }

                excelPackages.Add(data);
            }

            return excelPackages;
        }
    }

    public struct ExcelAxes
    {
        public int row;
        public int col;

        public ExcelAxes(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }

    public struct ExcelData
    {
        public ExcelAxes axes;
        public object data;
    }

    public struct ExcelPackageData
    {
        public string sheetName;
        public List<ExcelData> datas;
    }
}