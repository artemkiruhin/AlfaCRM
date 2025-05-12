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

        [HttpGet("businesses")]
        public async Task<IActionResult> GetBusinessStats(CancellationToken ct)
        {
            try
            {
                var result = await _statisticsService.GetUsersByTicketBusiness(ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {data = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
                var logsCount = await _database.LogRepository.CountAsync(ct);
                
                if (!usersAmount.IsSuccess) return BadRequest(usersAmount.ErrorMessage);
                if (!departmentsAmount.IsSuccess) return BadRequest(departmentsAmount.ErrorMessage);
                if (!problemCasesCount.IsSuccess) return BadRequest(problemCasesCount.ErrorMessage);
                if (!suggestionsCount.IsSuccess) return BadRequest(suggestionsCount.ErrorMessage);

                var result = new AdminStatisticsDTO(
                    DepartmentsAmount: departmentsAmount.Data,
                    ProblemCasesCount: problemCasesCount.Data.Item1,
                    SuggestionsCount: suggestionsCount.Data.Item1,
                    UsersAmount: usersAmount.Data,
                    SolvedProblemCasesCount: problemCasesCount.Data.Item2,
                    SolvedSuggestionsCount: suggestionsCount.Data.Item2,
                    LogsCount: logsCount
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

        [HttpGet("users/workload")]
        public async Task<IActionResult> GetUsersWorkload(CancellationToken ct)
        {
            try
            {
                var result = await _statisticsService.GetUsersByTicketBusiness(ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                return Ok(new { data = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("tickets/assign")]
        public async Task<IActionResult> AssignTicketToUser([FromBody] AssignTicketRequest request, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _statisticsService.DistributeTicketToUser(request.UserId, request.TicketId, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("tickets/distribute")]
        public async Task<IActionResult> DistributeTickets(CancellationToken ct)
        {
            try
            {
                var result = await _statisticsService.DistributeTicketsByUsers(ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                return Ok(new { success = true });
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

    public class AssignTicketRequest
    {
        public Guid UserId { get; set; }
        public Guid TicketId { get; set; }
    }
}