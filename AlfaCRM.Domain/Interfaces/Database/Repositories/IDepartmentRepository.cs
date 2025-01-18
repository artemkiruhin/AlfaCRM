using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IDepartmentRepository : ICrudRepository<DepartmentEntity>
{
    Task<DepartmentEntity?> GetByName(string name);
}