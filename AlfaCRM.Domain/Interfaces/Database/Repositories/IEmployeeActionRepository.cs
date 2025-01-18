using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IEmployeeActionRepository : ICrudRepository<EmployeeActionLogEntity>
{
    Task<IEnumerable<EmployeeActionLogEntity>> GetByEmployeeId(Guid employeeId, CancellationToken ct);
    Task<IEnumerable<EmployeeActionLogEntity>> GetByDescription(string description, CancellationToken ct);
    Task<IEnumerable<EmployeeActionLogEntity>> GetByDate(DateTime date, CancellationToken ct);
    Task<IEnumerable<EmployeeActionLogEntity>> GetByDate(DateTime from, DateTime to, CancellationToken ct);
}