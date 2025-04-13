using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IMessageRepository : ICrudRepository<MessageEntity>
{
    public Task<List<MessageEntity>> GetMessagesAsync(Guid chatId, CancellationToken ct);
}