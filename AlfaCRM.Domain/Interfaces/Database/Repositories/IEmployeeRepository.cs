using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IEmployeeRepository : ICrudRepository<EmployeeEntity>
{
    Task<EmployeeEntity?> GetByUsernameAndPassword(string username, string passwordHash, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByFullName(string fullName, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByEmail(string email, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByPhoneNumber(string phoneNumber, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByByRights(bool hasManagerRights, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByBirthday(DateTime date, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByBirthday(DateTime from, DateTime to, CancellationToken ct);
    
    Task<IEnumerable<EmployeeEntity>> GetByHiredDate(DateTime date, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByHiredDate(DateTime from, DateTime to, CancellationToken ct);
    
    Task<IEnumerable<EmployeeEntity>> GetByFiredDate(DateTime date, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByFiredDate(DateTime from, DateTime to, CancellationToken ct);
    
    Task<IEnumerable<EmployeeEntity>> GetByFired(bool isFired, CancellationToken ct);
    
    Task<IEnumerable<EmployeeEntity>> GetByAge(int age, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByAge(int from, int to, CancellationToken ct);
    
    Task<IEnumerable<EmployeeEntity>> GetByPosition(Guid id, CancellationToken ct);
    Task<IEnumerable<EmployeeEntity>> GetByDepartment(Guid id, CancellationToken ct);
}