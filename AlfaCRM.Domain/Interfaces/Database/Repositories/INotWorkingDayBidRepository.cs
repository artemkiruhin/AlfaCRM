using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface INotWorkingDayBidRepository : ICrudRepository<NotWorkingDayBidEntity>
{
    Task<IEnumerable<NotWorkingDayBidEntity>> GetByEmployee(Guid id, CancellationToken ct);
    Task<IEnumerable<NotWorkingDayBidEntity>> GetByEmployee(string username, CancellationToken ct);
    Task<IEnumerable<NotWorkingDayBidEntity>> GetByType(BidTypeEntity type, CancellationToken ct);
    Task<IEnumerable<NotWorkingDayBidEntity>> GetByPeriod(DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<NotWorkingDayBidEntity>> GetByCreated(DateTime from, DateTime to, CancellationToken ct);
}