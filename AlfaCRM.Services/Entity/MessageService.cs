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
                $"Starting message creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create message: sender with ID {request.SenderId} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Sender does not exist!");
            }

            var chat = await _database.ChatRepository.GetByIdAsync(request.ChatId, ct);
            if (chat == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create message: chat with ID {request.ChatId} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Chat does not exist!");
            }

            var isMember = await _database.ChatRepository.FindAsync(
                c => c.Id == request.ChatId && c.Members.Any(m => m.Id == request.SenderId),
                ct);

            if (isMember == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create message: sender {request.SenderId} is not a member of chat {request.ChatId}",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Sender is not a member of this chat");
            }

            MessageEntity? repliedMessage = null;
            if (request.RepliedMessageId.HasValue)
            {
                repliedMessage = await _database.MessageRepository.GetByIdAsync(request.RepliedMessageId.Value, ct);
                if (repliedMessage == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Failed to create message: replied message with ID {request.RepliedMessageId} not found",
                        request.SenderId), ct);
                    return Result<Guid>.Failure("Replied message does not exist!");
                }

                if (repliedMessage.ChatId != request.ChatId)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Failed to create message: replied message {request.RepliedMessageId} belongs to another chat (expected: {request.ChatId}, actual: {repliedMessage.ChatId})",
                        request.SenderId), ct);
                    return Result<Guid>.Failure("Replied message belongs to another chat");
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
                $"Message created successfully with ID: {newMessage.Id} in chat {request.ChatId}",
                request.SenderId), ct);

            return Result<Guid>.Success(newMessage.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating message: {e.Message}. StackTrace: {e.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Failed to create message: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(MessageUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting message update process for ID {request.Id}. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update message {request.Id}: empty content provided",
                    null), ct);
                return Result<Guid>.Failure("Message content cannot be empty");
            }

            var message = await _database.MessageRepository.GetByIdAsync(request.Id, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update message: message with ID {request.Id} not found",
                    null), ct);
                return Result<Guid>.Failure("Message does not exist!");
            }

            if (message.IsDeleted)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update message {request.Id}: message is deleted",
                    null), ct);
                return Result<Guid>.Failure("Cannot update deleted message");
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Updating message {request.Id} content from '{message.Content}' to '{request.Content}'",
                null), ct);

            message.Content = request.Content;
            message.UpdatedAt = DateTime.UtcNow;

            _database.MessageRepository.Update(message, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Message {request.Id} updated successfully",
                null), ct);

            return Result<Guid>.Success(message.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while updating message {request?.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Failed to update message: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, bool isLite, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting message deletion process for ID {id}. Lite mode: {isLite}",
                null), ct);

            var message = await _database.MessageRepository.GetByIdAsync(id, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete message: message with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("Message does not exist!");
            }

            if (isLite)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Performing lite deletion for message {id}",
                    null), ct);

                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;

                _database.MessageRepository.Update(message, ct);
            }
            else
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Performing hard deletion for message {id}",
                    null), ct);

                _database.MessageRepository.Delete(message, ct);
            }

            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Message {id} deleted successfully. Mode: {(isLite ? "Lite" : "Hard")}",
                null), ct);

            return Result<Guid>.Success(message.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting message {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Failed to delete message: {e.Message}");
        }
    }

    public async Task<Result<List<MessageDTO>>> GetAll(Guid chatId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve all messages for chat {chatId}",
                null), ct);

            var chatExists = await _database.ChatRepository.FindAsync(c => c.Id == chatId, ct);
            if (chatExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get messages: chat with ID {chatId} not found",
                    null), ct);
                return Result<List<MessageDTO>>.Failure("Chat does not exist");
            }

            var messages = await _database.MessageRepository.GetMessagesAsync(chatId, ct);
            var filteredMessages = messages.Where(m => m.IsDeleted == false).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Found {filteredMessages.Count} active messages in chat {chatId}",
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
                            DepartmentName: sender.Department?.Name ?? "No department",
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
                $"Successfully retrieved {dtos.Count} message DTOs for chat {chatId}",
                null), ct);

            return Result<List<MessageDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting messages for chat {chatId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<MessageDTO>>.Failure($"Failed to get messages: {e.Message}");
        }
    }

    public async Task<Result<MessageDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve message with ID {id}",
                null), ct);

            var message = await _database.MessageRepository.GetByIdAsync(id, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get message: message with ID {id} not found",
                    null), ct);
                return Result<MessageDTO>.Failure("Message not found");
            }

            if (message.IsDeleted)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get message: message {id} is deleted",
                    null), ct);
                return Result<MessageDTO>.Failure("Message is deleted");
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
                                DepartmentName: repliedMessageSender.Department?.Name ?? "No department",
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
                        DepartmentName: sender.Department?.Name ?? "No department",
                        IsAdmin: sender?.IsAdmin ?? false,
                        IsBlocked: sender?.IsBlocked ?? false),
                RepliedMessage: repliedMessageDto,
                Replies: new List<MessageDTO>(),
                IsOwn: sender?.Id == message.SenderId
            );

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved message with ID {id}",
                null), ct);

            return Result<MessageDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting message {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<MessageDTO>.Failure($"Failed to get message: {e.Message}");
        }
    }

    public async Task<Result<MessageDTO>> PinMessage(Guid messageId, bool isPinned, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to {(isPinned ? "pin" : "unpin")} message {messageId}",
                null), ct);

            var message = await _database.MessageRepository.GetByIdAsync(messageId, ct);
            if (message == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to pin message: message with ID {messageId} not found",
                    null), ct);
                return Result<MessageDTO>.Failure("Message not found");
            }

            if (message.IsDeleted)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to pin message {messageId}: message is deleted",
                    null), ct);
                return Result<MessageDTO>.Failure("Cannot pin deleted message");
            }

            message.IsPinned = isPinned;
            message.UpdatedAt = DateTime.UtcNow;

            _database.MessageRepository.Update(message, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Message {messageId} {(isPinned ? "pinned" : "unpinned")} successfully",
                null), ct);
            
            return await GetById(messageId, ct);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while pinning message {messageId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<MessageDTO>.Failure($"Failed to pin message: {e.Message}");
        }
    }
}