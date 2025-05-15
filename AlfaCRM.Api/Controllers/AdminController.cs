using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Interfaces.Services.Extensions;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ITicketService _ticketService;
        private readonly IStatisticsService _statisticsService;
        private readonly IUnitOfWork _database;

        public AdminController(IUserService userService, IDepartmentService departmentService, 
            ITicketService ticketService, IStatisticsService statisticsService, IUnitOfWork database)
        {
            _userService = userService;
            _departmentService = departmentService;
            _ticketService = ticketService;
            _statisticsService = statisticsService;
            _database = database;
        }
        
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs(CancellationToken ct)
        {
            try
            {
                var logs = await _database.LogRepository.GetAllAsync(ct);

                var dtos = logs.Select(x => new LogDTO(
                    Id: x.Id,
                    Message: x.Message,
                    Type: GetLogTypeString(x.Type),
                    UserIdString: x.UserId.HasValue ? x.UserId.Value.ToString() : "",
                    Username: x.User?.Username ?? "",
                    CreatedAt: x.CreatedAt
                ));

                return Ok(new { data = dtos });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GetLogTypeString(LogType logType)
        {
            return logType switch
            {
                LogType.Info => "Информация",
                LogType.Error => "Ошибка",
                LogType.Warning => "Предупреждение",
                _ => "Неизвестный тип"
            };
        }
    }
}