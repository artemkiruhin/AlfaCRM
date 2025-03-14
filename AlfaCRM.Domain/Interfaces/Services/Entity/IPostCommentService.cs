using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostCommentService
{
    Task<bool> Create(PostCreateRequest request);
    Task<bool> Update(PostUpdateRequest request);
    Task<bool> Delete(Guid id);
    
    Task<List<PostShortDTO>> GetAllShort();
    Task<List<PostDetailedDTO>> GetAll();
    Task<PostDetailedDTO> GetById(Guid id);
    Task<PostShortDTO> GetByIdShort(Guid id);
}