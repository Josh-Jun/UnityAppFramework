using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OfficeOpenXml;
using System.IO;

public class ExcelManager
{
    public static void WriteExcel(string path, List<ExcelPackageData> datas)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        FileStream fs = new FileStream(path, FileMode.Create);
        using (ExcelPackage package = new ExcelPackage(fs))
        {
            for (int i = 0; i < datas.Count; i++)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(datas[i].sheetName);
                for (int j = 0; j < datas[i].datas.Count; j++)
                {
                    ExcelData data = datas[i].datas[j];
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
        FileStream fs = new FileStream(path, FileMode.Open);
        List<ExcelPackageData> datas = new List<ExcelPackageData>();
        using (ExcelPackage package = new ExcelPackage(fs))
        {
            for (int i = 0; i < package.Workbook.Worksheets.Count; i++)
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets[i];
                ExcelPackageData data = new ExcelPackageData();
                data.sheetName = sheet.Name;
                for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                {
                    for (int m = sheet.Dimension.Start.Row, n = sheet.Dimension.End.Row; m <= n; m++)
                    {
                        ExcelData excelData = new ExcelData();
                        excelData.axes.row = m;
                        excelData.axes.col = j;
                        excelData.data = sheet.GetValue(m, j);
                        data.datas.Add(excelData);
                    }
                }
                datas.Add(data);
            }
        }
        return datas;
    }
}
public struct ExcelAxes
{
    public int row;
    public int col;
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