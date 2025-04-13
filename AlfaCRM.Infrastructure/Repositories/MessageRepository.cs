using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class MessageRepository(AppDbContext context) : BaseRepository<MessageEntity>(context), IMessageRepository
{
    public async Task<List<MessageEntity>> GetMessagesAsync(Guid chatId, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(c => c.Id == chatId).ToListAsync(ct);
    }
}