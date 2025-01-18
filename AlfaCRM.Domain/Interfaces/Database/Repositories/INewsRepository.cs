using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface INewsRepository : ICrudRepository<NewsEntity>
{
    Task<IEnumerable<NewsEntity>> GetByContent(string content, CancellationToken ct);
    Task<IEnumerable<NewsEntity>> GetByWatchers(int watchers, CancellationToken ct);
    Task<IEnumerable<NewsEntity>> GetByWatchers(int from, int to, CancellationToken ct);
    Task<IEnumerable<NewsEntity>> GetByCreated(DateTime date, CancellationToken ct);
    Task<IEnumerable<NewsEntity>> GetByCreated(DateTime from, DateTime to, CancellationToken ct);
}