using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Database.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlfaCRM.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction _currentTransaction;
    private bool _disposed = false;

    public UnitOfWork(
        AppDbContext context, 
        IDepartmentRepository departmentRepository, 
        IPostRepository postRepository,
        IUserRepository userRepository,
        IPostCommentRepository postCommentRepository,
        IPostReactionRepository postReactionRepository)
    {
        _context = context;
        DepartmentRepository = departmentRepository;
        PostRepository = postRepository;
        UserRepository = userRepository;
        PostCommentRepository = postCommentRepository;
        PostReactionRepository = postReactionRepository;
    }

    public IDepartmentRepository DepartmentRepository { get; }
    public IPostRepository PostRepository { get; }
    public IUserRepository UserRepository { get; }
    public IPostCommentRepository PostCommentRepository { get; }
    public IPostReactionRepository PostReactionRepository { get; }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction to commit");
        }

        try
        {
            await _context.SaveChangesAsync(ct);
            await _currentTransaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback");
        }

        try
        {
            await _currentTransaction.RollbackAsync(ct);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Не утилизируем _context, так как его жизненным циклом управляет DI
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            // Не утилизируем _context, так как его жизненным циклом управляет DI
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }

            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}