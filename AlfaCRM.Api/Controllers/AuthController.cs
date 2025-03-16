using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Interfaces.Services.Security;
using AlfaCRM.Domain.Models.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHashService _hasher;

        public AuthController(IUserService userService, IHashService hasher)
        {
            _userService = userService;
            _hasher = hasher;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest("Username is required");
                if (string.IsNullOrEmpty(request.PasswordHash) || string.IsNullOrWhiteSpace(request.PasswordHash))
                    return BadRequest("Password is required");
                
                var result = await _userService.Login(new LoginRequest(request.Username, _hasher.ComputeHash(request.PasswordHash)), ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                
                Response.Cookies.Append("jwt", result.Data.token, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(48),
                });
                
                return Ok(new { Id = result.Data.id, Token = result.Data.token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout(CancellationToken ct)
        {
            try
            {
                Response.Cookies.Delete("jwt");
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("validate")]
        [Authorize]
        public ActionResult Validate(CancellationToken ct)
        {
            return Ok();
        }
    }
}
