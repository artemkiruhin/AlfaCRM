using AlfaCRM.Api.Contracts.Request;
using AlfaCRM.Api.Extensions;
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
        private readonly IUserValidator _userValidator;

        public UsersController(IUserService userService, IUserValidator userValidator)
        {
            _userService = userService;
            _userValidator = userValidator;
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] UserCreateRequest request, CancellationToken ct)
        {
            try
            {
                var isUserValid = await _userValidator.IsAdmin(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest("Username is required");
                if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrWhiteSpace(request.FullName))
                    return BadRequest("Full name is required");
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
                var isUserValid = await _userValidator.IsAdmin(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
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
                    var userIdResult = await _userValidator.GetUserId(User, ct);
                    if (!userIdResult.IsSuccess) return Unauthorized();
                    var userId = userIdResult.Data;
                    if (userId != request.UserId) return Unauthorized();
                    
                    if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrWhiteSpace(request.OldPassword)) 
                        return BadRequest("Old password is required");
                    
                    var result = await _userService.ResetPassword(request.UserId, request.OldPassword, request.NewPassword, ct);
                    if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                    return Ok(new {id = result.Data});
                }
                
                var isUserAdmin = await _userValidator.IsAdmin(User, ct);
                if (!isUserAdmin.IsSuccess) return Unauthorized();
                
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
                var userIdResult = await _userValidator.GetUserId(User, ct);
                if (!userIdResult.IsSuccess) return Unauthorized();
                var userId = userIdResult.Data;
                // var isUserAdmin = await _userValidator.IsAdmin(User, ct);
                // if (!isUserAdmin.IsSuccess || userId != request.Id) return Unauthorized();
                //
                // if (request.IsAdmin.HasValue && request.IsAdmin.Value && !isUserAdmin.IsSuccess) return Unauthorized();
                //
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
                var userIdResult = await _userValidator.GetUserId(User, ct);
                if (!userIdResult.IsSuccess) return Unauthorized();
                var userId = userIdResult.Data;
                var isUserAdmin = await _userValidator.IsAdmin(User, ct);
                if (!isUserAdmin.IsSuccess) return Unauthorized();
                if (userId == id) return BadRequest("User cannot be deleted by himself");
                
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
                // var isUserValid = await _userValidator.IsAdmin(User, ct);
                // if (!isUserValid.IsSuccess) return Unauthorized();
                
                if (isShort is true)
                {
                    var shortResult = await _userService.GetAllShort(ct);
                    if (!shortResult.IsSuccess) return BadRequest(shortResult.ErrorMessage);
                    return Ok(new {users = shortResult.Data});
                }
                
                var detailedResult = await _userService.GetAll(ct);
                if (!detailedResult.IsSuccess) return BadRequest(detailedResult.ErrorMessage);
                return Ok(new {users = detailedResult.Data});
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
                // var isUserValid = await _userValidator.IsAdmin(User, ct);
                // if (!isUserValid.IsSuccess) return Unauthorized();
                
                if (isShort is true)
                {
                    var shortResult = await _userService.GetByIdShort(id, ct);
                    if (!shortResult.IsSuccess) return BadRequest(shortResult.ErrorMessage);
                    return Ok(new {user = shortResult.Data});
                }
                
                var detailedResult = await _userService.GetById(id, ct);
                if (!detailedResult.IsSuccess) return BadRequest(detailedResult.ErrorMessage);
                return Ok(new {user = detailedResult.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
