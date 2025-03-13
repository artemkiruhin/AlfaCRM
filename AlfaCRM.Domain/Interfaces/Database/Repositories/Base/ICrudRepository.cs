﻿namespace AlfaCRM.Domain.Interfaces.Database.Repositories.Base;

public interface ICrudRepository<TEntity> where TEntity: class
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> GetByIdAsync(Guid id);
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task<TEntity> DeleteAsync(TEntity entity);
}