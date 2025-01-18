namespace AlfaCRM.Domain.Interfaces.Database;

public interface IUnitOfWork : IDisposable
{
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task<int> SaveChangesAsync();
}