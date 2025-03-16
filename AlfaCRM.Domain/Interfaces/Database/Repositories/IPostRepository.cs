using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IPostRepository : ICrudRepository<PostEntity>
{
    Task<IEnumerable<PostEntity>> GetImportantPostsAsync(CancellationToken ct, bool isImportant, Guid? departmentId, bool includeDeleted = false);
    Task<IEnumerable<PostEntity>> GetActualPostsAsync(bool isActual, Guid? departmentId, CancellationToken ct);
    Task<IEnumerable<PostEntity>> GetPostForDepartment(Guid departmentId, CancellationToken ct);
    Task<IEnumerable<PostEntity>> GetCommonPosts(CancellationToken ct);
}