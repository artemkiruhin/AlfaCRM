using AlfaCRM.Domain.Interfaces.Services.Entity;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            try
            {
                var logs = await _logService.GetAll(ct);
                if (!logs.IsSuccess) return BadRequest(logs.ErrorMessage);
                return Ok(new {data = logs.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
