using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IChatService
{
    Task<Result<Guid>> Create(ChatCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Update(ChatUpdateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    
    Task<List<ChatShortDTO>> GetByNameAsync(string name, CancellationToken ct);
    Task<List<ChatShortDTO>> GetByUserAsync(Guid userId, CancellationToken ct);
    
    Task<Result<List<ChatShortDTO>>> GetAllShort(CancellationToken ct);
    Task<Result<List<ChatDetailedDTO>>> GetAll(CancellationToken ct);
    Task<Result<ChatDetailedDTO>> GetById(Guid id, CancellationToken ct);
    Task<Result<ChatShortDTO>> GetByIdShort(Guid id, CancellationToken ct);
}