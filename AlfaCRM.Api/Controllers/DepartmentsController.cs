using AlfaCRM.Api.Extensions;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IUserValidator _userValidator;

        public DepartmentsController(IDepartmentService departmentService, IUserValidator userValidator)
        {
            _departmentService = departmentService;
            _userValidator = userValidator;
        }

        [HttpGet("")]
        public async Task<ActionResult> GetAll(CancellationToken ct)
        {
            try
            {
                var detailedResult = await _departmentService.GetAll(ct);
                return detailedResult.IsSuccess 
                    ? Ok(new {departments = detailedResult.Data})
                    : BadRequest(detailedResult.ErrorMessage);
            }
            catch (OperationCanceledException)
            {
                // Specifically handle cancellation
                return StatusCode(499); // Client Closed Request
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet("short")]
        public async Task<ActionResult> GetAllShort(CancellationToken ct)
        {
            try
            {
                var shortResult = await _departmentService.GetAllShort(ct);
                return shortResult.IsSuccess
                    ? Ok(new { departments = shortResult.Data })
                    : BadRequest(shortResult.ErrorMessage);
            }
            catch (OperationCanceledException)
            {
                // Specifically handle cancellation
                return StatusCode(499); // Client Closed Request
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet("id/{id:guid}")]
        public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
        {
            try
            {
                var detailedResult = await _departmentService.GetById(id, ct);
                if (!detailedResult.IsSuccess) return BadRequest(detailedResult.ErrorMessage);
                return Ok(new {department = detailedResult.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] DepartmentCreateRequest request, CancellationToken ct)
        {
            try
            {
                var isUserValid = await _userValidator.IsAdmin(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
                var result = await _departmentService.Create(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {id = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPut("edit")]
        public async Task<ActionResult> Create([FromBody] DepartmentUpdateRequest request, CancellationToken ct)
        {
            try
            {
                var isUserValid = await _userValidator.IsAdmin(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
                var result = await _departmentService.Update(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {id = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpDelete("delete/{id:guid}")]
        public async Task<ActionResult> Create(Guid id, CancellationToken ct)
        {
            try
            {
                var isUserValid = await _userValidator.IsAdmin(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
                var result = await _departmentService.Delete(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {id = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
