using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _database;

    public ChatService(IUnitOfWork database)
    {
        _database = database;
    }

    private async Task<bool> AllUsersExist(List<Guid> usersIds, CancellationToken ct)
    {
        foreach (var id in usersIds)
        {
            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Пользователь с id {id} не найден при проверке",
                    null), ct);
                return false;
            }
        }

        return true;
    }

    public async Task<Result<Guid>> Create(ChatCreateRequest request, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса создания чата. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.Creator), ct);

            if (request.IsPersonal && request.MembersIds.Count != 1)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "Ошибка создания чата: попытка создания личного чата с != 1 собеседником",
                    request.Creator), ct);
                return Result<Guid>.Failure("Личный чат должен иметь 1 участника!");
            }

            var membersIdsIncludeCreator = new List<Guid>(request.MembersIds) { request.Creator };

            var usersExist = await AllUsersExist(membersIdsIncludeCreator, ct);
            if (!usersExist)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать чат: один или несколько пользователей не существуют. Участники: {string.Join(",", membersIdsIncludeCreator)}",
                    request.Creator), ct);
                return Result<Guid>.Failure("Как минимум один пользователь не существует!");
            }

            var name = "";
            if (request.IsPersonal)
            {
                var sender = await _database.UserRepository.GetByIdAsync(request.Creator, ct);
                if (sender == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Error,
                        $"Не удалось создать личный чат: создатель {request.Creator} не найден",
                        null), ct);
                    return Result<Guid>.Failure("Не удалось создать личный чат!");
                }

                var member = await _database.UserRepository.GetByIdAsync(request.MembersIds[0], ct);
                if (member == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Error,
                        $"Не удалось создать личный чат: участник {request.MembersIds[0]} не найден",
                        request.Creator), ct);
                    return Result<Guid>.Failure($"Пользователь с id {request.MembersIds[0]} не найден");
                }

                name = $"ЛС: {sender.Username} - {member.Username}";
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Сгенерировано имя личного чата: {name}",
                    request.Creator), ct);
            }
            else
            {
                name = request.Name;
            }

            var newChat = ChatEntity.Create(name, request.Creator);
            await _database.ChatRepository.CreateAsync(newChat, ct);
            await _database.SaveChangesAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Создана сущность чата с ID: {newChat.Id}",
                request.Creator), ct);

            foreach (var memberId in membersIdsIncludeCreator)
            {
                var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
                if (member == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Error,
                        $"Не удалось добавить участника {memberId} в чат {newChat.Id}",
                        request.Creator), ct);
                    return Result<Guid>.Failure($"Пользователь с id {memberId} не найден");
                }

                newChat.Members.Add(member);
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пользователь {memberId} добавлен в чат {newChat.Id}",
                    request.Creator), ct);
            }

            await _database.SaveChangesAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Чат успешно создан с ID: {newChat.Id}",
                request.Creator), ct);

            return Result<Guid>.Success(newChat.Id);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании чата: {e.Message}. StackTrace: {e.StackTrace}",
                request?.Creator), ct);
            return Result<Guid>.Failure($"Ошибка при создании чата: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(ChatUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса обновления чата с ID: {request.Id}",
                null), ct);

            if (string.IsNullOrEmpty(request.Name))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить чат {request.Id}: имя пустое",
                    null), ct);
                return Result<Guid>.Failure("Имя обязательно!");
            }

            var chatExist = await _database.ChatRepository.GetByIdAsync(request.Id, ct);
            if (chatExist == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить чат: чат с ID {request.Id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Чат не существует!");
            }

            chatExist.Name = request.Name;
            _database.ChatRepository.Update(chatExist, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Чат {request.Id} успешно обновлен",
                null), ct);

            return Result<Guid>.Success(chatExist.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при обновлении чата {request?.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении чата: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления чата с ID: {id}",
                null), ct);

            var dbChat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (dbChat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить чат: чат с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Чат не найден");
            }

            _database.ChatRepository.Delete(dbChat, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Чат {id} успешно удален",
                    null), ct);
                return Result<Guid>.Success(dbChat.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не удалось удалить чат {id}: изменения не сохранены",
                null), ct);
            return Result<Guid>.Failure("Не удалось удалить чат");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении чата {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при удалении чата: {ex.Message}");
        }
    }

    public async Task<Result<List<Guid>>> AddMembers(Guid chatId, List<Guid> memberIds, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса добавления участников в чат {chatId}. Участники: {string.Join(",", memberIds)}",
                null), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось добавить участников: чат с ID {chatId} не найден",
                    null), ct);
                return Result<List<Guid>>.Failure("Чат не найден");
            }

            var membersExist = await AllUsersExist(memberIds, ct);
            if (!membersExist)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось добавить участников в чат {chatId}: один или несколько пользователей не существуют",
                    null), ct);
                return Result<List<Guid>>.Failure("Как минимум один пользователь не существует!");
            }

            foreach (var member in memberIds)
                await PartAddMember(chat, member, ct);

            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Участники успешно добавлены в чат {chatId}",
                null), ct);

            return Result<List<Guid>>.Success(memberIds);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при добавлении участников в чат {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<Guid>>.Failure($"Ошибка при добавлении участников: {e.Message}");
        }
    }

    private async Task<Result<Guid>> PartAddMember(ChatEntity chat, Guid memberId, CancellationToken ct)
    {
        var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
        if (member == null)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не удалось добавить участника {memberId} в чат {chat.Id}: пользователь не найден",
                null), ct);
            return Result<Guid>.Failure("Пользователь не найден!");
        }

        if (chat.Members.Any(m => m.Id == memberId))
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Пользователь {memberId} уже является участником чата {chat.Id}",
                null), ct);
            return Result<Guid>.Failure("Пользователь уже является участником этого чата");
        }

        chat.Members.Add(member);
        _database.ChatRepository.Update(chat, ct);

        await _database.LogRepository.CreateAsync(LogEntity.Create(
            LogType.Info,
            $"Пользователь {memberId} успешно добавлен в чат {chat.Id}",
            null), ct);

        return Result<Guid>.Success(member.Id);
    }

    public async Task<Result<Guid>> AddMember(Guid chatId, Guid memberId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса добавления участника {memberId} в чат {chatId}",
                null), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось добавить участника {memberId}: чат с ID {chatId} не найден",
                    null), ct);
                return Result<Guid>.Failure("Чат не найден");
            }

            var result = await PartAddMember(chat, memberId, ct);
            if (!result.IsSuccess)
                return result;

            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Участник {memberId} успешно добавлен в чат {chatId}",
                null), ct);

            return result;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при добавлении участника {memberId} в чат {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при добавлении участника: {e.Message}");
        }
    }

    public async Task<Result<List<Guid>>> RemoveMember(Guid chatId, Guid memberId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления участника {memberId} из чата {chatId}",
                null), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить участника {memberId}: чат с ID {chatId} не найден",
                    null), ct);
                return Result<List<Guid>>.Failure("Чат не найден");
            }

            var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
            if (member == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить участника {memberId} из чата {chatId}: пользователь не найден",
                    null), ct);
                return Result<List<Guid>>.Failure("Пользователь не найден!");
            }

            if (chat.Admin?.Id == memberId)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить участника {memberId} из чата {chatId}: пользователь является администратором чата",
                    null), ct);
                return Result<List<Guid>>.Failure("Нельзя удалить администратора чата");
            }

            var memberToRemove = chat.Members.FirstOrDefault(m => m.Id == memberId);
            if (memberToRemove == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить участника {memberId} из чата {chatId}: пользователь не является участником",
                    null), ct);
                return Result<List<Guid>>.Failure("Пользователь не является участником этого чата");
            }

            chat.Members.Remove(memberToRemove);
            _database.ChatRepository.Update(chat, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Участник {memberId} успешно удален из чата {chatId}",
                null), ct);

            return Result<List<Guid>>.Success(chat.Members.Select(m => m.Id).ToList());
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении участника {memberId} из чата {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<Guid>>.Failure($"Ошибка при удалении участника: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetByNameAsync(string name, Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало поиска чатов по имени '{name}' для пользователя {userId}",
                userId), ct);

            var chats = await _database.ChatRepository.GetByNameAsync(name, ct);
            var dtos = chats.Select(x => MapToChatShortDTO(x, userId)).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Найдено {dtos.Count} чатов по имени '{name}' для пользователя {userId}",
                userId), ct);

            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении чатов по имени '{name}' для пользователя {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatShortDTO>>.Failure($"Ошибка при получении чатов: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetByUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало поиска чатов для пользователя {userId}",
                userId), ct);

            var user = await _database.UserRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить чаты для пользователя {userId}: пользователь не найден",
                    null), ct);
                return Result<List<ChatShortDTO>>.Failure("Пользователь не найден");
            }

            var chats = await _database.ChatRepository.GetByUserAsync(userId, ct);
            var dtos = chats.Select(x => MapToChatShortDTO(x, userId)).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Найдено {dtos.Count} чатов для пользователя {userId}",
                userId), ct);

            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении чатов для пользователя {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatShortDTO>>.Failure($"Ошибка при получении чатов: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetAllShort(Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения всех кратких чатов для пользователя {userId}",
                userId), ct);

            var chats = await _database.ChatRepository.GetAllAsync(ct);
            var dtos = chats.Select(x => MapToChatShortDTO(x, userId)).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} кратких чатов для пользователя {userId}",
                userId), ct);

            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении всех кратких чатов для пользователя {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatShortDTO>>.Failure($"Ошибка при получении чатов: {e.Message}");
        }
    }

    public async Task<Result<List<ChatDetailedDTO>>> GetAll(Guid? userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения всех детализированных чатов для пользователя {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);

            var chats = await _database.ChatRepository.GetAllAsync(ct);
            var dtos = chats.Select(chat => MapToChatDetailedDTO(chat, userId)).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} детализированных чатов для пользователя {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);

            return Result<List<ChatDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении всех детализированных чатов для пользователя {(userId.HasValue ? userId.ToString() : "system")}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatDetailedDTO>>.Failure($"Ошибка при получении чатов: {e.Message}");
        }
    }

    public async Task<Result<ChatDetailedDTO>> GetById(Guid id, Guid? userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения детализированного чата {id} для пользователя {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Чат {id} не найден для пользователя {(userId.HasValue ? userId.ToString() : "system")}",
                    userId), ct);
                return Result<ChatDetailedDTO>.Failure("Чат не найден");
            }

            var dto = MapToChatDetailedDTO(chat, userId);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Чат {id} успешно получен для пользователя {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);

            return Result<ChatDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении чата {id} для пользователя {(userId.HasValue ? userId.ToString() : "system")}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<ChatDetailedDTO>.Failure($"Ошибка при получении чата: {e.Message}");
        }
    }

    public async Task<Result<ChatShortDTO>> GetByIdShort(Guid id, Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения краткого чата {id} для пользователя {userId}",
                userId), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Краткий чат {id} не найден для пользователя {userId}",
                    userId), ct);
                return Result<ChatShortDTO>.Failure("Чат не найден");
            }

            var dto = MapToChatShortDTO(chat, userId);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Краткий чат {id} успешно получен для пользователя {userId}",
                userId), ct);

            return Result<ChatShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении краткого чата {id} для пользователя {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<ChatShortDTO>.Failure($"Ошибка при получении чата: {e.Message}");
        }
    }

    #region Mapping Methods

    private ChatShortDTO MapToChatShortDTO(ChatEntity chat, Guid? userId)
    {
        var chatName = chat.Members.Count == 2 && userId.HasValue
            ? chat.Members.First(x => x.Id != userId).Username
            : chat.Name;
        return new ChatShortDTO(
            Id: chat.Id,
            Name: chatName,
            CreatedAt: chat.CreatedAt,
            AdminId: chat.Admin?.Id
        );
    }

    private ChatDetailedDTO MapToChatDetailedDTO(ChatEntity chat, Guid? userId)
    {
        var chatName = chat.Members.Count == 2 && userId.HasValue
            ? chat.Members.First(x => x.Id != userId).Username
            : chat.Name;

        return new ChatDetailedDTO(
            Id: chat.Id,
            Name: chatName,
            CreatedAt: chat.CreatedAt,
            Admin: chat.Admin == null ? null : MapToUserShortDTO(chat.Admin),
            Messages: chat.Messages
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .Select(message => MapToMessageDTO(message, userId))
                .ToList(),
            Members: chat.Members.Select(MapToUserShortDTO).ToList()
        );
    }

    private MessageDTO MapToMessageDTO(MessageEntity message, Guid? userId)
    {
        return new MessageDTO(
            Id: message.Id,
            Content: message.Content,
            CreatedAt: message.CreatedAt,
            UpdatedAt: message.UpdatedAt,
            DeletedAt: message.DeletedAt,
            IsDeleted: message.IsDeleted,
            IsPinned: message.IsPinned,
            Sender: MapToUserShortDTO(message.Sender),
            RepliedMessage: message.RepliedMessage == null
                ? null
                : MapToRepliedMessageDTO(message.RepliedMessage, userId),
            Replies: new List<MessageDTO>(),
            IsOwn: userId.HasValue && userId.Value == message.SenderId
        );
    }

    private MessageDTO MapToRepliedMessageDTO(MessageEntity message, Guid? userId)
    {
        return new MessageDTO(
            Id: message.Id,
            Content: message.Content,
            CreatedAt: message.CreatedAt,
            UpdatedAt: message.UpdatedAt,
            DeletedAt: message.DeletedAt,
            IsDeleted: message.IsDeleted,
            IsPinned: message.IsPinned,
            Sender: MapToUserShortDTO(message.Sender),
            RepliedMessage: null,
            Replies: new List<MessageDTO>(),
            IsOwn: userId.HasValue && userId.Value == message.SenderId
        );
    }

    private UserShortDTO MapToUserShortDTO(UserEntity user)
    {
        return new UserShortDTO(
            Id: user.Id,
            FullName: user.FullName,
            Username: user.Username,
            Email: user.Email,
            DepartmentName: user.Department?.Name ?? "Нет отдела",
            IsAdmin: user?.IsAdmin ?? false,
            IsBlocked: user?.IsBlocked ?? false
        );
    }

    #endregion
}