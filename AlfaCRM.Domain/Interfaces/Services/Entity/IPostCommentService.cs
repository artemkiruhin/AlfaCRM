using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostCommentService
{
    Task<bool> Create(PostCommentCreateRequest request);
    Task<bool> Delete(Guid id);
    
    Task<List<PostCommentShortDTO>> GetAll(Guid postId);
    Task<PostCommentShortDTO> GetById(Guid id);
}