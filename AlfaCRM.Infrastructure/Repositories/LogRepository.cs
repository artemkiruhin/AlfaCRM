using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class LogRepository(AppDbContext context) : BaseRepository<LogEntity>(context), ILogRepository
{
    public async Task<IEnumerable<LogEntity>> GetAllByUserIdAsync(Guid? userId)
    {
        return userId.HasValue && userId != null
            ? await DbSet.AsNoTracking().Where(log => log.UserId.Value == userId.Value).ToListAsync()
            : await DbSet.AsNoTracking().Where(log => !log.UserId.HasValue).ToListAsync();
    }

    public async Task<IEnumerable<LogEntity>> GetAllByTypeAsync(LogType type)
    {
        return await DbSet.AsNoTracking().Where(log => log.Type == type).ToListAsync();
    }
}