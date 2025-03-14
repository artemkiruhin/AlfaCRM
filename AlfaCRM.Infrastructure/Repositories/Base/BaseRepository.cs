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
    
    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await DbSet.AsNoTracking().ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<TEntity> CreateAsync(TEntity entity)
    {
        var result = await DbSet.AddAsync(entity);
        return result.Entity;
    }

    public TEntity Update(TEntity entity)
    {
        Context.Attach(entity);
        Context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public TEntity Delete(TEntity entity)
    {
        var result = DbSet.Remove(entity);
        return result.Entity;
    }

    public async Task<IEnumerable<TEntity>> FindRangeAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync();
    }

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }
}