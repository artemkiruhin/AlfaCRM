using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IUserRepository : ICrudRepository<UserEntity>
{
    Task<UserEntity?> GetByUsernameAndPasswordAsync(string username, string passwordHash, CancellationToken ct);
    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken ct);
}