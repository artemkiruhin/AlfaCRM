using AlfaCRM.Domain.Interfaces.Database.Repositories;

namespace AlfaCRM.Domain.Interfaces.Database;

public interface IUnitOfWork
{
    public IDepartmentRepository DepartmentRepository { get; }
    public IPostRepository PostRepository { get; }
    public IUserRepository UserRepository { get; }
    public IPostCommentRepository PostCommentRepository { get; }
    public IPostReactionRepository PostReactionRepository { get; }
    public ITicketRepository TicketRepository { get; }
    public IChatRepository ChatRepository { get; }
    public IMessageRepository MessageRepository { get; }
    public ILogRepository LogRepository { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task BeginTransactionAsync(CancellationToken ct);
    Task CommitTransactionAsync(CancellationToken ct);
    Task RollbackTransactionAsync(CancellationToken ct);
}