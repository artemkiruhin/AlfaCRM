using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostCommentService
{
    Task<Result<Guid>> Create(PostCommentCreateRequest request);
    Task<Result<Guid>> Delete(Guid id);
    
    Task<Result<List<PostCommentShortDTO>>> GetAll(Guid postId);
    Task<Result<PostCommentShortDTO>> GetById(Guid id);
}