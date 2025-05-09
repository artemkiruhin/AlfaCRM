using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
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
        private readonly IUnitOfWork _database;

        public AdminController(IUserService userService, IDepartmentService departmentService, ITicketService ticketService, IUnitOfWork database)
        {
            _userService = userService;
            _departmentService = departmentService;
            _ticketService = ticketService;
            _database = database;
        }
        
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            try
            {
                var usersAmount = await _userService.GetUserCount(ct);
                var departmentsAmount = await _departmentService.GetDepartmentCount(ct);
                var problemCasesCount = await _ticketService.GetTicketsCount(TicketType.ProblemCase, ct);
                var suggestionsCount = await _ticketService.GetTicketsCount(TicketType.Suggestion, ct);

                if (!usersAmount.IsSuccess) return BadRequest(usersAmount.ErrorMessage);
                if (!departmentsAmount.IsSuccess) return BadRequest(departmentsAmount.ErrorMessage);
                if (!problemCasesCount.IsSuccess) return BadRequest(problemCasesCount.ErrorMessage);
                if (!suggestionsCount.IsSuccess) return BadRequest(problemCasesCount.ErrorMessage);
                
                var result = new AdminStatisticsDTO(
                    DepartmentsAmount: departmentsAmount.Data,
                    ProblemCasesCount: problemCasesCount.Data.Item1,
                    SuggestionsCount: suggestionsCount.Data.Item1,
                    UsersAmount: usersAmount.Data,
                    SolvedProblemCasesCount: problemCasesCount.Data.Item2,
                    SolvedSuggestionsCount: suggestionsCount.Data.Item2
                );

                return Ok(new { data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
                    Username: x.User.Username ?? "",
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
                LogType.Info => "Info",
                LogType.Error => "Error",
                LogType.Warning => "Warning",
                _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
            };
        }
    }
}
