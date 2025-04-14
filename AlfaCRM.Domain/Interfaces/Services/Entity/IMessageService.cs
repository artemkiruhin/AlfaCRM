using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IMessageService
{
    Task<Result<Guid>> Create(MessageCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Update(MessageUpdateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    
    Task<Result<List<MessageDTO>>> GetAll(Guid chatId, CancellationToken ct);
    Task<Result<MessageDTO>> GetById(Guid id, CancellationToken ct);
}