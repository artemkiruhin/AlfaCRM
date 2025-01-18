using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IPersonnelMovementRepository : ICrudRepository<PersonnelMovementEntity>
{
    Task<IEnumerable<PersonnelMovementEntity>> GetByEmployee(Guid id);
    Task<IEnumerable<PersonnelMovementEntity>> GetByEmployee(string username);
    Task<IEnumerable<PersonnelMovementEntity>> GetByHR(Guid id);
    Task<IEnumerable<PersonnelMovementEntity>> GetByHR(string username);
    Task<IEnumerable<PersonnelMovementEntity>> GetByPeriod(DateTime from, DateTime to);
    Task<IEnumerable<PersonnelMovementEntity>> GetHiredByDate(DateTime from, DateTime to);
    Task<IEnumerable<PersonnelMovementEntity>> GetFiredByDate(DateTime from, DateTime to);
    Task<IEnumerable<PersonnelMovementEntity>> GetByDate(DateTime from, DateTime to);
}