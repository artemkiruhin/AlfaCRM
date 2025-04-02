using AlfaCRM.Api.Contracts.Request;
using AlfaCRM.Api.Extensions;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IUserValidator _userValidator;

        public TicketsController(ITicketService ticketService, IUserValidator userValidator)
        {
            _ticketService = ticketService;
            _userValidator = userValidator;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(Guid? departmentId, bool isShort, CancellationToken ct)
        {
            try
            {
                var isAdminOrSpecialUser = await _userValidator.IsAdminOrSpecDepartment(User, ct);
                if (!isAdminOrSpecialUser.IsSuccess) return new UnauthorizedResult();
                
                if (isShort)
                {
                    var shortResult = await _ticketService.GetAllShort(departmentId, null, ct);
                    return Ok(new {data = shortResult});
                }
                
                var detailedResult = await _ticketService.GetAll(departmentId, null, ct);
                return Ok(new {data = detailedResult});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("my/{id:guid}")]
        public async Task<IActionResult> GetAllByUserId([FromRoute] Guid id, bool isShort, CancellationToken ct)
        {
            try
            {
                var validateResult = await _userValidator.HasRightsToWorkWithTickets(User, null, id, ct);
                if (!validateResult.IsSuccess) return Unauthorized();
                
                var userId = await _userValidator.GetUserId(User, ct);
                if (!userId.IsSuccess) return Unauthorized();
                
                if (isShort)
                {
                    var shortResult = await _ticketService.GetAllShort(null, userId.Data, ct);
                    return Ok(new {data = shortResult});
                }
                
                var detailedResult = await _ticketService.GetAll(null, userId.Data, ct);
                return Ok(new {data = detailedResult});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TicketCreateApiRequest request, CancellationToken ct)
        {
            try
            {
                var userId = await _userValidator.GetUserId(User, ct);
                if (!userId.IsSuccess) return Unauthorized();

                var result = await _ticketService.Create(new TicketCreateRequest(
                    Title: request.Title,
                    Text: request.Text,
                    DepartmentId: request.DepartmentId,
                    userId.Data
                ), ct);
                
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                return Ok(new {data = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPut("edit")]
        public async Task<IActionResult> Edit([FromBody] TicketUpdateApiRequest request, CancellationToken ct)
        {
            try
            {
                var userId = await _userValidator.GetUserId(User, ct);
                if (!userId.IsSuccess) return Unauthorized();

                var result = await _ticketService.Update(new TicketUpdateRequest(
                    Id: request.Id,
                    SenderId: userId.Data,
                    Title: request.Title,
                    Text: request.Text,
                    DepartmentId: request.DepartmentId,
                    Feedback: request.Feedback
                ), ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                return Ok(new {data = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPatch("change-status")]
        public async Task<IActionResult> Edit([FromBody] TicketChangeStatusApiRequest request, CancellationToken ct)
        {
            try
            {
                var userId = await _userValidator.GetUserId(User, ct);
                if (!userId.IsSuccess) return Unauthorized();

                if (request.Status is TicketStatus.Completed or TicketStatus.Rejected && request.Feedback is null)
                    throw new ArgumentException();
                
                var result = request.Status switch
                {
                    TicketStatus.Created => throw new ArgumentException(),
                    TicketStatus.InWork => await _ticketService.TakeToWork(request.Id, userId.Data, ct),
                    TicketStatus.Completed => await _ticketService.Complete(request.Id, userId.Data, request.Feedback, ct),
                    TicketStatus.Rejected => await _ticketService.Reject(request.Id, userId.Data, request.Feedback, ct),
                    _ => throw new ArgumentException()
                };
                
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                return Ok(new {data = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var isAdminOrSender = await _userValidator.HasRightsToWorkWithTickets(User, null, id, ct);
                if (!isAdminOrSender.IsSuccess) return Unauthorized();
                
                var userId = await _userValidator.GetUserId(User, ct);
                if (!userId.IsSuccess) return Unauthorized();

                var result = await _ticketService.Delete(id, userId.Data, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                return Ok(new {data = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
