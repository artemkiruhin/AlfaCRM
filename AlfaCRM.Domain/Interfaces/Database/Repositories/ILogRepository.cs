using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface ILogRepository : ICrudRepository<LogEntity>
{
    Task<IEnumerable<LogEntity>> GetByMessage(string message, CancellationToken ct);
    Task<IEnumerable<LogEntity>> GetByType(LogTypeEntity logType, CancellationToken ct);
    Task<IEnumerable<LogEntity>> GetByDate(DateTime from, DateTime to, CancellationToken ct);
}