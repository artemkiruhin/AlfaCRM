using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IPersonnelMovementRepository : ICrudRepository<PersonnelMovementEntity>
{
    Task<IEnumerable<PersonnelMovementEntity>> GetByEmployee(Guid id, CancellationToken ct);
    Task<IEnumerable<PersonnelMovementEntity>> GetByEmployee(string username, CancellationToken ct);
    Task<IEnumerable<PersonnelMovementEntity>> GetByHR(Guid id, CancellationToken ct);
    Task<IEnumerable<PersonnelMovementEntity>> GetByHR(string username, CancellationToken ct);
    Task<IEnumerable<PersonnelMovementEntity>> GetByPeriod(DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<PersonnelMovementEntity>> GetHiredByDate(DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<PersonnelMovementEntity>> GetFiredByDate(DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<PersonnelMovementEntity>> GetByDate(DateTime from, DateTime to, CancellationToken ct);
}