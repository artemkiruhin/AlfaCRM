using System.Data;

namespace AlfaCRM.Domain.Models.DTOs;
public class ExcelReportRequest
{
    public string DocumentTitle { get; set; }
    public string Description { get; set; }
    public List<ReportTable> Tables { get; set; } = new List<ReportTable>();
    public bool AutoFitColumns { get; set; } = true;
    public Dictionary<string, string> ColumnFormats { get; set; } = new Dictionary<string, string>();
}