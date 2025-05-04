using AlfaCRM.Api.Contracts.Request;
using AlfaCRM.Services.Report;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly CreateReportUseCase _createReportUseCase;

    public ReportsController(CreateReportUseCase createReportUseCase)
    {
        _createReportUseCase = createReportUseCase;
    }

    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] ReportCreateApiRequest request, CancellationToken ct)
    {
        try
        {
            Console.WriteLine(request.Table + " " + request.Title);
            var fileContentResult = await _createReportUseCase.Exec(request.Table, request.Description, ct);
            if (!fileContentResult.IsSuccess) return BadRequest(fileContentResult.ErrorMessage);
            var fileContent = fileContentResult.Data;
            
            return File(fileContent, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"{request.Title}.xlsx");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Error generating Excel: {ex.Message}" });
        }
    }
}