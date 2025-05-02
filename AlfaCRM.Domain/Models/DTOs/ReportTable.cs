namespace AlfaCRM.Domain.Models.DTOs;

public class ReportTable
{
    public string TableName { get; set; }
    public List<ReportColumn> Columns { get; set; } = new List<ReportColumn>();
    public List<List<object>> Rows { get; set; } = new List<List<object>>();
}