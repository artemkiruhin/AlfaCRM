using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<UserEntity>(context), IUserRepository
{
    public async Task<UserEntity?> GetByUsernameAndPasswordAsync(string username, string passwordHash, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(user => user.Username == username && user.PasswordHash == passwordHash, ct);
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(user => user.Username == username, ct);
    }
}