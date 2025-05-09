using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface ILogRepository : ICrudRepository<LogEntity>
{
    Task<IEnumerable<LogEntity>> GetAllByUserIdAsync(Guid? userId);
    Task<IEnumerable<LogEntity>> GetAllByTypeAsync(LogType type);
}