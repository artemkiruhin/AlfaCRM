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
                    $"User with id {id} not found during validation",
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
                $"Starting chat creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.Creator), ct);

            if (request.IsPersonal && request.MembersIds.Count != 1)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "Ошибка создания чата: попытка создания беседы с != 1 собеседником",
                    request.Creator), ct);
                return Result<Guid>.Failure("Personal chat must have 1 member!");
            }

            var membersIdsIncludeCreator = new List<Guid>(request.MembersIds) { request.Creator };
        
            var usersExist = await AllUsersExist(membersIdsIncludeCreator, ct);
            if (!usersExist)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create chat: one or more users don't exist. Members: {string.Join(",", membersIdsIncludeCreator)}",
                    request.Creator), ct);
                return Result<Guid>.Failure("At least one user does not exist!");
            }

            var name = "";
            if (request.IsPersonal)
            {
                var sender = await _database.UserRepository.GetByIdAsync(request.Creator, ct);
                if (sender == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Error,
                        $"Failed to create personal chat: creator {request.Creator} not found",
                        null), ct);
                    return Result<Guid>.Failure("Cannot create a personal chat!");
                }
                
                var member = await _database.UserRepository.GetByIdAsync(request.MembersIds[0], ct);
                if (member == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Error,
                        $"Failed to create personal chat: member {request.MembersIds[0]} not found",
                        request.Creator), ct);
                    return Result<Guid>.Failure($"User with id {request.MembersIds[0]} not found");
                }

                name = $"P: {sender.Username} - {member.Username}";
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Personal chat name generated: {name}",
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
                $"Chat entity created with ID: {newChat.Id}",
                request.Creator), ct);

            foreach (var memberId in membersIdsIncludeCreator)
            {
                var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
                if (member == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Error,
                        $"Failed to add member {memberId} to chat {newChat.Id}",
                        request.Creator), ct);
                    return Result<Guid>.Failure($"User with id {memberId} not found");
                }

                newChat.Members.Add(member);
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"User {memberId} added to chat {newChat.Id}",
                    request.Creator), ct);
            }

            await _database.SaveChangesAsync(ct);
        
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Chat successfully created with ID: {newChat.Id}",
                request.Creator), ct);
            
            return Result<Guid>.Success(newChat.Id);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating chat: {e.Message}. StackTrace: {e.StackTrace}",
                request?.Creator), ct);
            return Result<Guid>.Failure($"Error while creating chat: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(ChatUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting chat update process for chat ID: {request.Id}",
                null), ct);

            if (string.IsNullOrEmpty(request.Name))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update chat {request.Id}: name is empty",
                    null), ct);
                return Result<Guid>.Failure("Name is required!");
            }
            
            var chatExist = await _database.ChatRepository.GetByIdAsync(request.Id, ct);
            if (chatExist == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update chat: chat with ID {request.Id} not found",
                    null), ct);
                return Result<Guid>.Failure("Chat doesn't exist!");
            }
            
            chatExist.Name = request.Name;
            _database.ChatRepository.Update(chatExist, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Chat {request.Id} successfully updated",
                null), ct);
            
            return Result<Guid>.Success(chatExist.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while updating chat {request?.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while updating chat: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting chat deletion process for chat ID: {id}",
                null), ct);

            var dbChat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (dbChat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete chat: chat with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("Chat not found");
            }
            
            _database.ChatRepository.Delete(dbChat, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Chat {id} successfully deleted",
                    null), ct);
                return Result<Guid>.Success(dbChat.Id);
            }
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Failed to delete chat {id}: no changes saved",
                null), ct);
            return Result<Guid>.Failure("Failed to delete chat");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting chat {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while deleting chat: {ex.Message}");
        }
    }

    public async Task<Result<List<Guid>>> AddMembers(Guid chatId, List<Guid> memberIds, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting process to add members to chat {chatId}. Members: {string.Join(",", memberIds)}",
                null), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to add members: chat with ID {chatId} not found",
                    null), ct);
                return Result<List<Guid>>.Failure("Chat not found");
            }

            var membersExist = await AllUsersExist(memberIds, ct);
            if (!membersExist)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to add members to chat {chatId}: one or more users don't exist",
                    null), ct);
                return Result<List<Guid>>.Failure("At least one user is not exist!");
            }
            
            foreach (var member in memberIds) 
                await PartAddMember(chat, member, ct);
            
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully added members to chat {chatId}",
                null), ct);
            
            return Result<List<Guid>>.Success(memberIds);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while adding members to chat {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<Guid>>.Failure($"Error while adding members: {e.Message}");
        }
    }

    private async Task<Result<Guid>> PartAddMember(ChatEntity chat, Guid memberId, CancellationToken ct)
    {
        var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
        if (member == null)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Failed to add member {memberId} to chat {chat.Id}: user not found",
                null), ct);
            return Result<Guid>.Failure("User not found!");
        }

        if (chat.Members.Any(m => m.Id == memberId))
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"User {memberId} is already a member of chat {chat.Id}",
                null), ct);
            return Result<Guid>.Failure("User is already a member of this chat");
        }

        chat.Members.Add(member);
        _database.ChatRepository.Update(chat, ct);
        
        await _database.LogRepository.CreateAsync(LogEntity.Create(
            LogType.Info,
            $"User {memberId} successfully added to chat {chat.Id}",
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
                $"Starting process to add member {memberId} to chat {chatId}",
                null), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to add member {memberId}: chat with ID {chatId} not found",
                    null), ct);
                return Result<Guid>.Failure("Chat not found");
            }
            
            var result = await PartAddMember(chat, memberId, ct);
            if (!result.IsSuccess)
                return result;
                
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Member {memberId} successfully added to chat {chatId}",
                null), ct);
            
            return result;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while adding member {memberId} to chat {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while adding member: {e.Message}");
        }
    }
    
    public async Task<Result<List<Guid>>> RemoveMember(Guid chatId, Guid memberId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting process to remove member {memberId} from chat {chatId}",
                null), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to remove member {memberId}: chat with ID {chatId} not found",
                    null), ct);
                return Result<List<Guid>>.Failure("Chat not found");
            }
            
            var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
            if (member == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to remove member {memberId} from chat {chatId}: user not found",
                    null), ct);
                return Result<List<Guid>>.Failure("User not found!");
            }
            
            if (chat.Admin?.Id == memberId)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to remove member {memberId} from chat {chatId}: user is chat admin",
                    null), ct);
                return Result<List<Guid>>.Failure("Cannot remove chat admin");
            }
            
            var memberToRemove = chat.Members.FirstOrDefault(m => m.Id == memberId);
            if (memberToRemove == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to remove member {memberId} from chat {chatId}: user is not a member",
                    null), ct);
                return Result<List<Guid>>.Failure("User is not a member of this chat");
            }
            
            chat.Members.Remove(memberToRemove);
            _database.ChatRepository.Update(chat, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Member {memberId} successfully removed from chat {chatId}",
                null), ct);
            
            return Result<List<Guid>>.Success(chat.Members.Select(m => m.Id).ToList());
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while removing member {memberId} from chat {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<Guid>>.Failure($"Error while removing member: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetByNameAsync(string name, Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting search for chats by name '{name}' for user {userId}",
                userId), ct);

            var chats = await _database.ChatRepository.GetByNameAsync(name, ct);
            var dtos = chats.Select(x => MapToChatShortDTO(x, userId)).ToList();
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Found {dtos.Count} chats by name '{name}' for user {userId}",
                userId), ct);
            
            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting chats by name '{name}' for user {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatShortDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetByUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting search for chats for user {userId}",
                userId), ct);

            var user = await _database.UserRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get chats for user {userId}: user not found",
                    null), ct);
                return Result<List<ChatShortDTO>>.Failure("User not found");
            }
            
            var chats = await _database.ChatRepository.GetByUserAsync(userId, ct);
            var dtos = chats.Select(x => MapToChatShortDTO(x, userId)).ToList();
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Found {dtos.Count} chats for user {userId}",
                userId), ct);
            
            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting chats for user {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatShortDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetAllShort(Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to get all short chats for user {userId}",
                userId), ct);

            var chats = await _database.ChatRepository.GetAllAsync(ct);
            var dtos = chats.Select(x => MapToChatShortDTO(x, userId)).ToList();
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} short chats for user {userId}",
                userId), ct);
            
            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting all short chats for user {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatShortDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<List<ChatDetailedDTO>>> GetAll(Guid? userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to get all detailed chats for user {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);

            var chats = await _database.ChatRepository.GetAllAsync(ct);
            var dtos = chats.Select(chat => MapToChatDetailedDTO(chat, userId)).ToList();
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} detailed chats for user {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);
            
            return Result<List<ChatDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting all detailed chats for user {(userId.HasValue ? userId.ToString() : "system")}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<List<ChatDetailedDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<ChatDetailedDTO>> GetById(Guid id, Guid? userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to get detailed chat {id} for user {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Chat {id} not found for user {(userId.HasValue ? userId.ToString() : "system")}",
                    userId), ct);
                return Result<ChatDetailedDTO>.Failure("Chat not found");
            }

            var dto = MapToChatDetailedDTO(chat, userId);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved chat {id} for user {(userId.HasValue ? userId.ToString() : "system")}",
                userId), ct);
            
            return Result<ChatDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting chat {id} for user {(userId.HasValue ? userId.ToString() : "system")}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<ChatDetailedDTO>.Failure($"Error while getting chat: {e.Message}");
        }
    }

    public async Task<Result<ChatShortDTO>> GetByIdShort(Guid id, Guid userId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to get short chat {id} for user {userId}",
                userId), ct);

            var chat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Short chat {id} not found for user {userId}",
                    userId), ct);
                return Result<ChatShortDTO>.Failure("Chat not found");
            }

            var dto = MapToChatShortDTO(chat, userId);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved short chat {id} for user {userId}",
                userId), ct);
            
            return Result<ChatShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting short chat {id} for user {userId}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<ChatShortDTO>.Failure($"Error while getting chat: {e.Message}");
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