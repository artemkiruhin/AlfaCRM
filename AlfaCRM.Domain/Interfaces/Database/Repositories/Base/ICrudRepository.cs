using System.Linq.Expressions;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories.Base;

public interface ICrudRepository<TEntity>
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken ct);
    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken ct);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
    Task<TEntity> GetByIdAsync(Guid id);
}