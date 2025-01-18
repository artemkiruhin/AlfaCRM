using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IPositionRepository : ICrudRepository<PositionEntity>
{
    Task<PositionEntity?> GetByName(string name);
    Task<IEnumerable<PositionEntity>> GetBySalary(decimal salary);
}