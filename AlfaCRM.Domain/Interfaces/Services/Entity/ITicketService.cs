using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface ITicketService
{
    Task<Result<Guid>> Create(TicketCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Update(TicketUpdateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    
    Task<Result<List<TicketShortDTO>>> GetAllShort(Guid? departmentId, Guid? senderId, CancellationToken ct);
    Task<Result<List<TicketDetailedDTO>>> GetAll(Guid? departmentId, Guid? senderId, CancellationToken ct);
    Task<Result<TicketDetailedDTO>> GetById(Guid id, CancellationToken ct);
    Task<Result<TicketShortDTO>> GetByIdShort(Guid id, CancellationToken ct);
    
    Task<Result<Guid>> TakeToWork (Guid id, CancellationToken ct);
    Task<Result<Guid>> Complete (Guid id, string feedback, CancellationToken ct);
    Task<Result<Guid>> Reject (Guid id, string feedback, CancellationToken ct);
}