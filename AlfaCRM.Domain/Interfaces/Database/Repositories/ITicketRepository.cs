using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface ITicketRepository : ICrudRepository<TicketEntity>
{
    Task<IEnumerable<TicketEntity>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken ct);
    Task<IEnumerable<TicketEntity>> GetByCreatorIdAsync(Guid creatorId, CancellationToken ct);
}