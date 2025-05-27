using System.Security.Claims;
using AlfaCRM.Api.Contracts.Request;
using AlfaCRM.Api.Extensions;
using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly IPostReactionService _postReactionService;
        private readonly IPostCommentService _postCommentService;
        private readonly IUserService _userService;
        private readonly IUserValidator _userValidator;

        public PostsController(IPostService postService, IPostReactionService postReactionService, IPostCommentService postCommentService, IUserService userService, IUserValidator userValidator)
        {
            _postService = postService;
            _postReactionService = postReactionService;
            _postCommentService = postCommentService;
            _userService = userService;
            _userValidator = userValidator;
        }

        [HttpGet("")]
        public async Task<ActionResult> GetPosts([FromQuery] Guid? departmentId, CancellationToken ct)
        {
            try
            {
                var result = await _postService.GetAllShort(departmentId, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage);
                return Ok(new { posts = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("id/{id:guid}")]
        public async Task<ActionResult> GetPostById(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _postService.GetById(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {post = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut("edit")]
        public async Task<ActionResult> Edit([FromBody] PostUpdateRequest request, CancellationToken ct)
        {
            try
            {
                var isUserValid = await _userValidator.IsAdminOrPublisher(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
                if (string.IsNullOrEmpty(request.Title)
                    && string.IsNullOrEmpty(request.Subtitle)
                    && string.IsNullOrEmpty(request.Content)
                    && !request.IsImportant.HasValue
                    && !request.DepartmentId.HasValue)
                    return BadRequest("At least 1 property must be changed");
                
                var result = await _postService.Update(request, ct);
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
                var isUserValid = await _userValidator.IsAdminOrPublisher(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
                var result = await _postService.Delete(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("block/{id:guid}")]
        public async Task<ActionResult> Block(Guid id, CancellationToken ct)
        {
            try
            {
                var isUserValid = await _userValidator.IsAdminOrPublisher(User, ct);
                if (!isUserValid.IsSuccess) return Unauthorized();
                
                var result = await _postService.Block(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("create")]
        public async Task<ActionResult> Create([FromBody] PostCreateApiRequest request, CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var userId = Guid.Parse(userIdClaim);

                var req = new PostCreateRequest(
                    request.Title,
                    request.Subtitle,
                    request.Content,
                    request.IsImportant,
                    request.DepartmentId,
                    userId
                );
                
                var result = await _postService.Create(req, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("react")]
        public async Task<ActionResult> CreateReact([FromBody] PostReactionCreateApiRequest request, CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var userId = Guid.Parse(userIdClaim);

                var req = new PostReactionCreateRequest(
                    request.PostId,
                    userId,
                    request.Type
                );
                
                var result = await _postReactionService.Create(req, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("delete-react/{id:guid}")]
        public async Task<ActionResult> DeleteReact(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _postReactionService.Delete(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("delete-post-all-reacts/{id:guid}")]
        public async Task<ActionResult> DeleteAllPostReacts(Guid id, CancellationToken ct)
        {
            try
            {
                var userId = await _userValidator.GetUserId(User, ct);
                if (!userId.IsSuccess) return BadRequest(userId.ErrorMessage);
                
                var result = await _postReactionService.DeleteAll(id, userId.Data, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("add-comment")]
        public async Task<ActionResult> CreateComment([FromBody] PostCommentCreateApiRequest request, CancellationToken ct)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var userId = Guid.Parse(userIdClaim);

                var req = new PostCommentCreateRequest(
                    request.Content,
                    request.PostId,
                    userId
                );
                
                var result = await _postCommentService.Create(req, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("remove-comment/{id:guid}")]
        public async Task<ActionResult> DeleteComment(Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _postCommentService.Delete(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
