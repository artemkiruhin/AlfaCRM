using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostService
{
    Task<Result<Guid>> Create(PostCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Update(PostUpdateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    
    Task<Result<List<PostShortDTO>>> GetAllShort(Guid? departmentId, CancellationToken ct);
    Task<Result<List<PostDetailedDTO>>> GetAll(Guid? departmentId, CancellationToken ct);
    Task<Result<PostDetailedDTO>> GetById(Guid id, CancellationToken ct);
    Task<Result<PostShortDTO>> GetByIdShort(Guid id, CancellationToken ct);
    
    Task<Result<Guid>> Block(Guid id, CancellationToken ct);
}