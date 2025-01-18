using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IFeedbackRepository : ICrudRepository<FeedbackEntity>
{
    Task<IEnumerable<FeedbackEntity>> GetByContent(string content, CancellationToken ct);
    Task<IEnumerable<FeedbackEntity>> GetByEmail(string email, CancellationToken ct);
    Task<IEnumerable<FeedbackEntity>> GetByType(FeedbackType type, CancellationToken ct);
    Task<IEnumerable<FeedbackEntity>> GetByDate(DateTime date, CancellationToken ct);
    Task<IEnumerable<FeedbackEntity>> GetByDate(DateTime from, DateTime to, CancellationToken ct);
}