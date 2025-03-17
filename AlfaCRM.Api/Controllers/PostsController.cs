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

        public PostsController(IPostService postService, IPostReactionService postReactionService, IPostCommentService postCommentService)
        {
            _postService = postService;
            _postReactionService = postReactionService;
            _postCommentService = postCommentService;
        }

        [HttpGet("")]
        public async Task<ActionResult> GetPosts(Guid? departmentId, CancellationToken ct)
        {
            try
            {
                var result = await _postService.GetAllShort(departmentId, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {posts = result.Data});
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
        
        [HttpDelete("delete")]
        public async Task<ActionResult> Delete([FromBody] Guid id, CancellationToken ct)
        {
            try
            {
                var result = await _postService.Delete(id, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("block")]
        public async Task<ActionResult> Block([FromBody] Guid id, CancellationToken ct)
        {
            try
            {
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
        public async Task<ActionResult> Create([FromBody] PostCreateRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _postService.Create(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("react")]
        public async Task<ActionResult> CreateReact([FromBody] PostReactionCreateRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _postReactionService.Create(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("delete-react")]
        public async Task<ActionResult> DeleteReact([FromBody] Guid id, CancellationToken ct)
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
        
        [HttpDelete("add-comment")]
        public async Task<ActionResult> CreateComment([FromBody] PostCommentCreateRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _postCommentService.Create(request, ct);
                if (!result.IsSuccess) return BadRequest(result.ErrorMessage); 
                return Ok(new {id = result.Data});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("remove-comment")]
        public async Task<ActionResult> DeleteComment([FromBody] Guid id, CancellationToken ct)
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
