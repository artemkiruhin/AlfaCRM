using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostCommentService
{
    Task<Result<Guid>> Create(PostCommentCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    
    Task<Result<List<PostCommentShortDTO>>> GetAll(Guid postId, CancellationToken ct);
    Task<Result<PostCommentShortDTO>> GetById(Guid id, CancellationToken ct);
}