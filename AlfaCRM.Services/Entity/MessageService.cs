using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _database;

    public MessageService(IUnitOfWork database)
    {
        _database = database;
    }

    public async Task<Result<Guid>> Create(MessageCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса создания сообщения. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать сообщение: отправитель с ID {request.SenderId} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Отправитель не существует!");
            }

            var chat = await _database.ChatRepository.GetByIdAsync(request.ChatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать сообщение: чат с ID {request.ChatId} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Чат не существует!");
            }

            var isMember = await _database.ChatRepository.FindAsync(
                c => c.Id == request.ChatId && c.Members.Any(m => m.Id == request.SenderId),
                ct);

            if (isMember == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать сообщение: отправитель {request.SenderId} не является участником чата {request.ChatId}",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Отправитель не является участником этого чата");
            }

            MessageEntity? repliedMessage = null;
            if (request.RepliedMessageId.HasValue)
            {
                repliedMessage = await _database.MessageRepository.GetByIdAsync(request.RepliedMessageId.Value, ct);
                if (repliedMessage == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Не удалось создать сообщение: ответное сообщение с ID {request.RepliedMessageId} не найдено",
                        request.SenderId), ct);
                    return Result<Guid>.Failure("Ответное сообщение не существует!");
                }

                if (repliedMessage.ChatId != request.ChatId)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Не удалось создать сообщение: ответное сообщение {request.RepliedMessageId} принадлежит другому чату (ожидалось: {request.ChatId}, фактически: {repliedMessage.ChatId})",
                        request.SenderId), ct);
                    return Result<Guid>.Failure("Ответное сообщение принадлежит другому чату");
                }
            }

            var newMessage = MessageEntity.Create(
                request.Content,
                request.SenderId,
                repliedMessage?.Id,
                request.ChatId);

            await _database.MessageRepository.CreateAsync(newMessage, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Сообщение успешно создано с ID: {newMessage.Id} в чате {request.ChatId}",
                request.SenderId), ct);

            return Result<Guid>.Success(newMessage.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании сообщения: {e.Message}. StackTrace: {e.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Не удалось создать сообщение: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(MessageUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса обновления сообщения с ID {request.Id}. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить сообщение {request.Id}: пустое содержимое",
                    null), ct);
                return Result<Guid>.Failure("Содержимое сообщения не может быть пустым");
            }

            var message = await _database.MessageRepository.GetByIdAsync(request.Id, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить сообщение: сообщение с ID {request.Id} не найдено",
                    null), ct);
                return Result<Guid>.Failure("Сообщение не существует!");
            }

            if (message.IsDeleted)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить сообщение {request.Id}: сообщение удалено",
                    null), ct);
                return Result<Guid>.Failure("Нельзя обновить удаленное сообщение");
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Обновление содержимого сообщения {request.Id} с '{message.Content}' на '{request.Content}'",
                null), ct);

            message.Content = request.Content;
            message.UpdatedAt = DateTime.UtcNow;

            _database.MessageRepository.Update(message, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Сообщение {request.Id} успешно обновлено",
                null), ct);

            return Result<Guid>.Success(message.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при обновлении сообщения {request?.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Не удалось обновить сообщение: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, bool isLite, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления сообщения с ID {id}. Режим: {(isLite ? "Lite" : "Hard")}",
                null), ct);

            var message = await _database.MessageRepository.GetByIdAsync(id, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить сообщение: сообщение с ID {id} не найдено",
                    null), ct);
                return Result<Guid>.Failure("Сообщение не существует!");
            }

            if (isLite)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Выполнение мягкого удаления для сообщения {id}",
                    null), ct);

                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;

                _database.MessageRepository.Update(message, ct);
            }
            else
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Выполнение полного удаления для сообщения {id}",
                    null), ct);

                _database.MessageRepository.Delete(message, ct);
            }

            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Сообщение {id} успешно удалено. Режим: {(isLite ? "Lite" : "Hard")}",
                null), ct);

            return Result<Guid>.Success(message.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении сообщения {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Не удалось удалить сообщение: {e.Message}");
        }
    }

    public async Task<Result<List<MessageDTO>>> GetAll(Guid chatId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения всех сообщений для чата {chatId}",
                null), ct);

            var chatExists = await _database.ChatRepository.FindAsync(c => c.Id == chatId, ct);
            if (chatExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить сообщения: чат с ID {chatId} не найден",
                    null), ct);
                return Result<List<MessageDTO>>.Failure("Чат не существует");
            }

            var messages = await _database.MessageRepository.GetMessagesAsync(chatId, ct);
            var filteredMessages = messages.Where(m => m.IsDeleted == false).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Найдено {filteredMessages.Count} активных сообщений в чате {chatId}",
                null), ct);

            var senderIds = filteredMessages.Select(m => m.SenderId).Distinct();
            var senders = await _database.UserRepository.FindRangeAsync(
                u => senderIds.Contains(u.Id),
                ct);

            var repliedMessageIds = filteredMessages
                .Where(m => m.RepliedMessageId.HasValue)
                .Select(m => m.RepliedMessageId.Value)
                .Distinct();

            var messageIds = repliedMessageIds as Guid[] ?? repliedMessageIds.ToArray();
            var repliedMessages = messageIds.Any()
                ? await _database.MessageRepository.FindRangeAsync(
                    m => messageIds.Contains(m.Id),
                    ct)
                : new List<MessageEntity>();

            var dtos = filteredMessages.Select(message =>
            {
                var sender = senders.FirstOrDefault(s => s.Id == message.SenderId);
                var repliedMessage = repliedMessages.FirstOrDefault(rm => rm.Id == message.RepliedMessageId);

                return new MessageDTO(
                    Id: message.Id,
                    Content: message.Content,
                    CreatedAt: message.CreatedAt,
                    UpdatedAt: message.UpdatedAt,
                    DeletedAt: message.DeletedAt,
                    IsDeleted: message.IsDeleted,
                    IsPinned: message.IsPinned,
                    Sender: sender == null
                        ? null
                        : new UserShortDTO(
                            Id: sender.Id,
                            FullName: sender.FullName,
                            Username: sender.Username,
                            Email: sender.Email,
                            DepartmentName: sender.Department?.Name ?? "Без отдела",
                            IsAdmin: sender?.IsAdmin ?? false,
                            IsBlocked: sender?.IsBlocked ?? false),
                    RepliedMessage: repliedMessage == null
                        ? null
                        : new MessageDTO(
                            Id: repliedMessage.Id,
                            Content: repliedMessage.Content,
                            CreatedAt: repliedMessage.CreatedAt,
                            UpdatedAt: repliedMessage.UpdatedAt,
                            DeletedAt: repliedMessage.DeletedAt,
                            IsDeleted: repliedMessage.IsDeleted,
                            IsPinned: repliedMessage.IsPinned,
                            Sender: null,
                            RepliedMessage: null,
                            Replies: new List<MessageDTO>(),
                            IsOwn: sender?.Id == message.SenderId
                        ),
                    Replies: new List<MessageDTO>(),
                    IsOwn: sender?.Id == message.SenderId
                );
            }).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Успешно получено {dtos.Count} DTO сообщений для чата {chatId}",
                null), ct);

            return Result<List<MessageDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении сообщений для чата {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<MessageDTO>>.Failure($"Не удалось получить сообщения: {e.Message}");
        }
    }

    public async Task<Result<MessageDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения сообщения с ID {id}",
                null), ct);

            var message = await _database.MessageRepository.GetByIdAsync(id, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить сообщение: сообщение с ID {id} не найдено",
                    null), ct);
                return Result<MessageDTO>.Failure("Сообщение не найдено");
            }

            if (message.IsDeleted)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить сообщение: сообщение {id} удалено",
                    null), ct);
                return Result<MessageDTO>.Failure("Сообщение удалено");
            }

            var sender = await _database.UserRepository.GetByIdAsync(message.SenderId, ct);

            MessageDTO? repliedMessageDto = null;
            if (message.RepliedMessageId.HasValue)
            {
                var repliedMessage = await _database.MessageRepository.GetByIdAsync(message.RepliedMessageId.Value, ct);
                if (repliedMessage != null && !repliedMessage.IsDeleted)
                {
                    var repliedMessageSender = await _database.UserRepository.GetByIdAsync(repliedMessage.SenderId, ct);

                    repliedMessageDto = new MessageDTO(
                        Id: repliedMessage.Id,
                        Content: repliedMessage.Content,
                        CreatedAt: repliedMessage.CreatedAt,
                        UpdatedAt: repliedMessage.UpdatedAt,
                        DeletedAt: repliedMessage.DeletedAt,
                        IsDeleted: repliedMessage.IsDeleted,
                        IsPinned: repliedMessage.IsPinned,
                        Sender: repliedMessageSender == null
                            ? null
                            : new UserShortDTO(
                                Id: repliedMessageSender.Id,
                                FullName: repliedMessageSender.FullName,
                                Username: repliedMessageSender.Username,
                                Email: repliedMessageSender.Email,
                                DepartmentName: repliedMessageSender.Department?.Name ?? "Без отдела",
                                IsAdmin: repliedMessageSender?.IsAdmin ?? false,
                                IsBlocked: repliedMessageSender?.IsBlocked ?? false),
                        RepliedMessage: null,
                        Replies: new List<MessageDTO>(),
                        IsOwn: sender?.Id == message.SenderId
                    );
                }
            }

            var dto = new MessageDTO(
                Id: message.Id,
                Content: message.Content,
                CreatedAt: message.CreatedAt,
                UpdatedAt: message.UpdatedAt,
                DeletedAt: message.DeletedAt,
                IsDeleted: message.IsDeleted,
                IsPinned: message.IsPinned,
                Sender: sender == null
                    ? null
                    : new UserShortDTO(
                        Id: sender.Id,
                        FullName: sender.FullName,
                        Username: sender.Username,
                        Email: sender.Email,
                        DepartmentName: sender.Department?.Name ?? "Без отдела",
                        IsAdmin: sender?.IsAdmin ?? false,
                        IsBlocked: sender?.IsBlocked ?? false),
                RepliedMessage: repliedMessageDto,
                Replies: new List<MessageDTO>(),
                IsOwn: sender?.Id == message.SenderId
            );

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Успешно получено сообщение с ID {id}",
                null), ct);

            return Result<MessageDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении сообщения {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<MessageDTO>.Failure($"Не удалось получить сообщение: {e.Message}");
        }
    }

    public async Task<Result<MessageDTO>> PinMessage(Guid messageId, bool isPinned, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса {(isPinned ? "закрепления" : "открепления")} сообщения {messageId}",
                null), ct);

            var message = await _database.MessageRepository.GetByIdAsync(messageId, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось закрепить сообщение: сообщение с ID {messageId} не найдено",
                    null), ct);
                return Result<MessageDTO>.Failure("Сообщение не найдено");
            }

            if (message.IsDeleted)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось закрепить сообщение {messageId}: сообщение удалено",
                    null), ct);
                return Result<MessageDTO>.Failure("Нельзя закрепить удаленное сообщение");
            }

            message.IsPinned = isPinned;
            message.UpdatedAt = DateTime.UtcNow;

            _database.MessageRepository.Update(message, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Сообщение {messageId} успешно {(isPinned ? "закреплено" : "откреплено")}",
                null), ct);

            return await GetById(messageId, ct);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при закреплении сообщения {messageId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<MessageDTO>.Failure($"Не удалось закрепить сообщение: {e.Message}");
        }
    }
}