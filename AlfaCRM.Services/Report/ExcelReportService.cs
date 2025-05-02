using ClosedXML.Excel;
using System.Data;
using AlfaCRM.Domain.Models.DTOs;

public class ExcelReportService
{
    public byte[] GenerateExcelReport(ExcelReportRequest request)
    {
        using var workbook = new XLWorkbook();
        foreach (var table in ConvertToDataTables(request.Tables))
        {
            var worksheet = workbook.Worksheets.Add(table.TableName ?? "Sheet1");
                
            worksheet.Style.Font.FontName = "Inter";
            worksheet.Style.Font.FontSize = 11;
                
            AddTitleSection(worksheet, request);
                
            AddColumnHeaders(worksheet, table);
                
            AddDataRows(worksheet, table, request.ColumnFormats);

            if (!request.AutoFitColumns) continue;
            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Where(c => c.Width > 50).ToList().ForEach(c => c.Width = 50);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private List<DataTable> ConvertToDataTables(List<ReportTable> reportTables)
    {
        var dataTables = new List<DataTable>();

        foreach (var reportTable in reportTables)
        {
            var dataTable = new DataTable(reportTable.TableName);
            
            foreach (var column in reportTable.Columns)
            {
                dataTable.Columns.Add(column.ColumnName, GetColumnType(column.DataType));
            }
            
            foreach (var row in reportTable.Rows)
            {
                var dataRow = dataTable.NewRow();
                for (int i = 0; i < row.Count; i++)
                {
                    dataRow[i] = ConvertValue(row[i], reportTable.Columns[i].DataType);
                }
                dataTable.Rows.Add(dataRow);
            }

            dataTables.Add(dataTable);
        }

        return dataTables;
    }

    private Type GetColumnType(string dataType)
    {
        return dataType.ToLower() switch
        {
            "number" => typeof(decimal),
            "date" => typeof(DateTime),
            "boolean" => typeof(bool),
            _ => typeof(string)
        };
    }

    private object? ConvertValue(object value, string dataType)
    {
        if (value == null) return DBNull.Value;

        try
        {
            return dataType.ToLower() switch
            {
                "number" => Convert.ToDecimal(value),
                "date" => Convert.ToDateTime(value),
                "boolean" => Convert.ToBoolean(value),
                _ => value.ToString()
            };
        }
        catch
        {
            return value.ToString();
        }
    }

    private void AddTitleSection(IXLWorksheet worksheet, ExcelReportRequest request)
    {
        var titleCell = worksheet.Cell(1, 1);
        titleCell.Value = request.DocumentTitle;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 18;
        titleCell.Style.Font.FontColor = XLColor.FromHtml("#1f2937");
        titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        worksheet.Range(1, 1, 1, worksheet.Columns().Count()).Merge();
        
        var descCell = worksheet.Cell(2, 1);
        descCell.Value = request.Description;
        descCell.Style.Font.FontColor = XLColor.FromHtml("#4b5563");
        descCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        descCell.Style.Font.Italic = true;
        worksheet.Range(2, 1, 2, worksheet.Columns().Count()).Merge();
    }

    private void AddColumnHeaders(IXLWorksheet worksheet, DataTable table)
    {
        var headerRow = worksheet.Row(4);
        
        for (int i = 0; i < table.Columns.Count; i++)
        {
            var cell = headerRow.Cell(i + 1);
            cell.Value = table.Columns[i].ColumnName;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4f46e5");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }
        
        worksheet.SheetView.Freeze(4, 0);
    }

    private void AddDataRows(IXLWorksheet worksheet, DataTable table, Dictionary<string, string> formats)
    {
        for (int rowIdx = 0; rowIdx < table.Rows.Count; rowIdx++)
        {
            var row = table.Rows[rowIdx];
            var xlRow = worksheet.Row(rowIdx + 5);
            
            xlRow.Style.Fill.BackgroundColor = rowIdx % 2 == 0 
                ? XLColor.White 
                : XLColor.FromHtml("#f9fafb");

            for (int colIdx = 0; colIdx < table.Columns.Count; colIdx++)
            {
                var cell = xlRow.Cell(colIdx + 1);
                var value = row[colIdx];
                
                if (value == DBNull.Value)
                {
                    cell.Value = string.Empty;
                    continue;
                }

                var columnName = table.Columns[colIdx].ColumnName;
                
                if (formats.TryGetValue(columnName, out string format))
                {
                    if (value is DateTime dt)
                        cell.Value = dt.ToString(format);
                    else if (value is decimal dec)
                        cell.Value = dec.ToString(format);
                    else
                        cell.Value = value.ToString();
                }
                else
                {
                    cell.Value = value.ToString();
                }
                
                if (table.Columns[colIdx].DataType == typeof(decimal))
                {
                    cell.Style.NumberFormat.Format = "#,##0.00";
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
            }
        }
    }
}