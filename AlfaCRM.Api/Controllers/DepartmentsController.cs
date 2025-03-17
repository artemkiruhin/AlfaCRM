using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet("")]
        public async Task<ActionResult> GetAll(bool? isShort, CancellationToken ct)
        {
            try
            {
                if (isShort is true)
                {
                    var shortResult = await _departmentService.GetAllShort(ct);
                    if (!shortResult.IsSuccess) return BadRequest(shortResult.ErrorMessage);
                    return Ok(new {departments = shortResult.Data});
                }
                
                var detailedResult = await _departmentService.GetAll(ct);
                if (!detailedResult.IsSuccess) return BadRequest(detailedResult.ErrorMessage);
                return Ok(new {departments = detailedResult.Data});
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
                var result = await _departmentService.Update(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {id = result.Data});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpDelete("delete")]
        public async Task<ActionResult> Create([FromBody] Guid id, CancellationToken ct)
        {
            try
            {
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
