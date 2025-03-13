using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IPostRepository : ICrudRepository<PostEntity>
{
    Task<IEnumerable<PostEntity>> GetImportantPostsAsync(bool isImportant, Guid? departmentId, bool includeDeleted = false);
    Task<IEnumerable<PostEntity>> GetActualPostsAsync(bool isActual, Guid? departmentId);
    Task<IEnumerable<PostEntity>> GetPostForDepartment(Guid departmentId);
    Task<IEnumerable<PostEntity>> GetCommonPosts();
}