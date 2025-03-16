using System.Linq.Expressions;
using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories.Base;

public class BaseRepository<TEntity> : ICrudRepository<TEntity> where TEntity : class
{
    protected AppDbContext Context { get; }
    protected DbSet<TEntity> DbSet { get; }

    public BaseRepository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }
    
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await DbSet.FindAsync(new object[] { id }, ct);
    }

    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct)
    {
        var result = await DbSet.AddAsync(entity, ct);
        return result.Entity;
    }

    public TEntity Update(TEntity entity, CancellationToken ct)
    {
        Context.Attach(entity);
        Context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public TEntity Delete(TEntity entity, CancellationToken ct)
    {
        var result = DbSet.Remove(entity);
        return result.Entity;
    }

    public async Task<IEnumerable<TEntity>> FindRangeAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(ct);
    }

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct);
    }
}