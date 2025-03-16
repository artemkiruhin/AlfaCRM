using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IPostCommentRepository : ICrudRepository<PostCommentEntity>
{
    Task<IEnumerable<PostCommentEntity>> GetDeletedCommentsAsync(Guid postId, CancellationToken ct);
}