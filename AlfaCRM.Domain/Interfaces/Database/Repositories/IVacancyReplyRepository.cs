using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IVacancyReplyRepository : ICrudRepository<VacancyReplyEntity>
{
    Task<IEnumerable<VacancyReplyEntity>> GetByName(string name, CancellationToken ct);
    Task<IEnumerable<VacancyReplyEntity>> GetByPhone(string phoneNumber, CancellationToken ct);
    Task<IEnumerable<VacancyReplyEntity>> GetByEmail(string email, CancellationToken ct);
    Task<IEnumerable<VacancyReplyEntity>> GetByDescriptionContent(string descriptionContent, CancellationToken ct);
    Task<VacancyReplyEntity?> GetByFileId(Guid id, CancellationToken ct);
    Task<IEnumerable<VacancyReplyEntity>> GetByDate(DateTime from, DateTime to, CancellationToken ct);
}