using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostService
{
    Task<bool> Create(PostCreateRequest request);
    Task<bool> Update(PostUpdateRequest request);
    Task<bool> Delete(Guid id);
    
    Task<List<PostShortDTO>> GetAllShort(Guid? departmentId);
    Task<List<PostDetailedDTO>> GetAll(Guid? departmentId);
    Task<PostDetailedDTO> GetById(Guid id);
    Task<PostShortDTO> GetByIdShort(Guid id);
    
    Task<bool> Block(Guid id);
}