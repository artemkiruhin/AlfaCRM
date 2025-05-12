using System.Collections;
using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IDepartmentRepository : ICrudRepository<DepartmentEntity>
{
    Task<DepartmentEntity?> GetDepartmentByName(string name, CancellationToken ct);
    Task<IEnumerable<DepartmentEntity>> GetDepartmentsBySpecific(bool isSpecific, CancellationToken ct);
}