using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface INewsCommentRepository : ICrudRepository<NewsCommentEntity>
{
    Task<List<NewsCommentEntity>> GetByNewsId(Guid id, CancellationToken ct);
    Task<List<NewsCommentEntity>> GetByDate(DateTime from , DateTime to, CancellationToken ct);
}