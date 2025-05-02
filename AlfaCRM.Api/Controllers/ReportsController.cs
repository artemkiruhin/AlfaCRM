using AlfaCRM.Domain.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ExcelReportService _excelReportService;

    public ReportsController(ExcelReportService excelReportService)
    {
        _excelReportService = excelReportService;
    }

    [HttpPost("export-excel")]
    public IActionResult ExportExcel([FromBody] ExcelReportRequest request)
    {
        try
        {
            var fileContent = _excelReportService.GenerateExcelReport(request);
            return File(fileContent, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"{request.DocumentTitle}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Error generating Excel: {ex.Message}" });
        }
    }
}