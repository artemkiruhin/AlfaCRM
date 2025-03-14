using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Database.Repositories;

namespace AlfaCRM.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
{
    private readonly AppDbContext _context;

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


    public async Task<int> SaveChangesAsync(CancellationToken ct) => 
        await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct) => 
        await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct) =>
        await _context.Database.CommitTransactionAsync(ct);

    public async Task RollbackTransactionAsync(CancellationToken ct) =>
        await _context.Database.RollbackTransactionAsync(ct);

    public void Dispose() => _context.Dispose();

    public async ValueTask DisposeAsync() => await _context.DisposeAsync();
    
}