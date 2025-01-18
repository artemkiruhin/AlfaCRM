using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IPositionRepository : ICrudRepository<PositionEntity>
{
    Task<IEnumerable<PositionEntity>> GetByName(string name, CancellationToken ct);
    Task<IEnumerable<PositionEntity>> GetBySalary(decimal salary, CancellationToken ct);
}