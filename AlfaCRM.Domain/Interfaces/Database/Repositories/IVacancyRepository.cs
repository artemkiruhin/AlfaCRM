using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IVacancyRepository : ICrudRepository<VacancyEntity>
{
    Task<IEnumerable<VacancyEntity>> GetByContent(string content, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetBySalary(decimal salary, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetBySalary(decimal from, decimal to, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByDate(DateTime created, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByDate(DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByRemoteAllow(bool allowsRemote, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByExperience(int experience, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByExperience(int from, int to, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByWatchers(int watchers, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByWatchers(int from, int to, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByReplyQuantity(int quantity, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByReplyQuantity(int from, int to, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByPublisher(string username, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByPublisher(Guid id, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByType(VacancyType type, CancellationToken ct);
    Task<IEnumerable<VacancyEntity>> GetByShiftType(ShiftType type, CancellationToken ct);
}