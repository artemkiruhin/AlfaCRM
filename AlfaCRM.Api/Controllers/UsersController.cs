using AlfaCRM.Api.Contracts.Request;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] UserCreateRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest("Username is required");
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest("Email is required");
                if (string.IsNullOrEmpty(request.PasswordHash) || string.IsNullOrWhiteSpace(request.PasswordHash))
                    return BadRequest("Password is required");

                var result = await _userService.Create(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("block/{id:guid}")]
        public async Task<ActionResult> Create(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _userService.Block(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordBodyRequest request, CancellationToken ct)
        {
            try
            {
                if (request.MustValidate)
                {
                    if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrWhiteSpace(request.OldPassword)) 
                        return BadRequest("Old password is required");
                    
                    var result = await _userService.ResetPassword(request.UserId, request.OldPassword, request.NewPassword, ct);
                    if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                    return Ok(new {id = result.Data});
                }
                
                var adminResult = await _userService.ResetPasswordAsAdmin(request.UserId, request.NewPassword, ct);
                if (!adminResult.IsSuccess) return BadRequest(adminResult.ErrorMessage);
                return Ok(new {id = adminResult.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPatch("edit")]
        public async Task<ActionResult> Edit([FromBody] UserUpdateRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email)
                    && !request.IsAdmin.HasValue
                    && !request.HasPublishedRights.HasValue
                    && !request.DepartmentId.HasValue)
                    return BadRequest("At least 1 property must be changed");

                var result = await _userService.Update(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("delete/{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _userService.Delete(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("")]
        public async Task<ActionResult> GetAll(bool? isShort, CancellationToken ct)
        {
            try
            {
                if (isShort is true)
                {
                    var shortResult = await _userService.GetAllShort(ct);
                    if (!shortResult.IsSuccess) return BadRequest(shortResult.ErrorMessage);
                    return Ok(new {id = shortResult.Data});
                }
                
                var detailedResult = await _userService.GetAll(ct);
                if (!detailedResult.IsSuccess) return BadRequest(detailedResult.ErrorMessage);
                return Ok(new {id = detailedResult.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("id/{id:guid}")]
        public async Task<ActionResult> GetAll([FromRoute] Guid id, [FromQuery] bool? isShort, CancellationToken ct)
        {
            try
            {
                if (isShort is true)
                {
                    var shortResult = await _userService.GetByIdShort(id, ct);
                    if (!shortResult.IsSuccess) return BadRequest(shortResult.ErrorMessage);
                    return Ok(new {id = shortResult.Data});
                }
                
                var detailedResult = await _userService.GetById(id, ct);
                if (!detailedResult.IsSuccess) return BadRequest(detailedResult.ErrorMessage);
                return Ok(new {id = detailedResult.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
