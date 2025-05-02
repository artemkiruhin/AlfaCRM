namespace AlfaCRM.Domain.Models.DTOs;

public class ReportColumn
{
    public string ColumnName { get; set; }
    public string DataType { get; set; } // "string", "number", "date", "boolean"
}