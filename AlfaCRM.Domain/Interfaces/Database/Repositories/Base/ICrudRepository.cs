using System.Linq.Expressions;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories.Base;

public interface ICrudRepository<TEntity> where TEntity: class
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(Guid id);
    Task<TEntity> CreateAsync(TEntity entity);
    TEntity Update(TEntity entity);
    TEntity Delete(TEntity entity);
    Task<IEnumerable<TEntity>> FindRangeAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate);
}