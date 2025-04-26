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

        public AdminController(IUserService userService, IDepartmentService departmentService, ITicketService ticketService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _ticketService = ticketService;
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
    }
}
