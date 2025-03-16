using System.Linq.Expressions;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories.Base;

public interface ICrudRepository<TEntity> where TEntity: class
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct);
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct);
    TEntity Update(TEntity entity, CancellationToken ct);
    TEntity Delete(TEntity entity, CancellationToken ct);
    Task<IEnumerable<TEntity>> FindRangeAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
}