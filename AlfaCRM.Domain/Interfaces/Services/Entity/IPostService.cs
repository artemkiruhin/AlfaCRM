using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostService
{
    Task<Result<Guid>> Create(PostCreateRequest request);
    Task<Result<Guid>> Update(PostUpdateRequest request);
    Task<Result<Guid>> Delete(Guid id);
    
    Task<Result<List<PostShortDTO>>> GetAllShort(Guid? departmentId);
    Task<Result<List<PostDetailedDTO>>> GetAll(Guid? departmentId);
    Task<Result<PostDetailedDTO>> GetById(Guid id);
    Task<Result<PostShortDTO>> GetByIdShort(Guid id);
    
    Task<Result<Guid>> Block(Guid id);
}