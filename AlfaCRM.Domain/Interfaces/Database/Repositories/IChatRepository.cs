using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IChatRepository : ICrudRepository<ChatEntity>
{
    public Task<List<ChatEntity>> GetByNameAsync(string name, CancellationToken ct);
    public Task<List<ChatEntity>> GetByUserAsync(Guid userId, CancellationToken ct);
}