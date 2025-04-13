using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class ChatRepository(AppDbContext context) : BaseRepository<ChatEntity>(context), IChatRepository
{
    public async Task<List<ChatEntity>> GetByNameAsync(string name, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(c => c.Name.Contains(name)).ToListAsync(ct);
    }

    public async Task<List<ChatEntity>> GetByUserAsync(Guid userId, CancellationToken ct)
    {
        return await DbSet.AsNoTracking()
            .Where(c => c.Members.FirstOrDefault(m => m.Id == userId) != null)
            .ToListAsync(ct);
    }
}