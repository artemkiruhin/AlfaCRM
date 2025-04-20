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
            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null) 
                return Result<Guid>.Failure("Sender does not exist!");

            var chat = await _database.ChatRepository.GetByIdAsync(request.ChatId, ct);
            if (chat == null) 
                return Result<Guid>.Failure("Chat does not exist!");
            
            var isMember = await _database.ChatRepository.FindAsync(
                c => c.Id == request.ChatId && c.Members.Any(m => m.Id == request.SenderId),
                ct);
            
            if (isMember == null)
                return Result<Guid>.Failure("Sender is not a member of this chat");

            MessageEntity? repliedMessage = null;
            if (request.RepliedMessageId.HasValue)
            {
                repliedMessage = await _database.MessageRepository.GetByIdAsync(request.RepliedMessageId.Value, ct);
                if (repliedMessage == null) 
                    return Result<Guid>.Failure("Replied message does not exist!");
                
                if (repliedMessage.ChatId != request.ChatId)
                    return Result<Guid>.Failure("Replied message belongs to another chat");
            }

            var newMessage = MessageEntity.Create(
                request.Content,
                request.SenderId,
                repliedMessage?.Id,
                request.ChatId);

            await _database.MessageRepository.CreateAsync(newMessage, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return Result<Guid>.Success(newMessage.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Failed to create message: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(MessageUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return Result<Guid>.Failure("Message content cannot be empty");

            var message = await _database.MessageRepository.GetByIdAsync(request.Id, ct);
            if (message == null)
                return Result<Guid>.Failure("Message does not exist!");

            if (message.IsDeleted)
                return Result<Guid>.Failure("Cannot update deleted message");
            
            message.Content = request.Content;
            message.UpdatedAt = DateTime.UtcNow;

            _database.MessageRepository.Update(message, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return Result<Guid>.Success(message.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Failed to update message: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, bool isLite, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var message = await _database.MessageRepository.GetByIdAsync(id, ct);
            if (message == null)
                return Result<Guid>.Failure("Message does not exist!");

            if (isLite)
            {
                message.IsDeleted = true;
                message.DeletedAt = DateTime.UtcNow;

                _database.MessageRepository.Update(message, ct);
            }
            else
            {
                _database.MessageRepository.Delete(message, ct);
            }
            
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return Result<Guid>.Success(message.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Failed to delete message: {e.Message}");
        }
    }

    public async Task<Result<List<MessageDTO>>> GetAll(Guid chatId, CancellationToken ct)
    {
        try
        {
            var chatExists = await _database.ChatRepository.FindAsync(c => c.Id == chatId, ct);
            if (chatExists == null)
                return Result<List<MessageDTO>>.Failure("Chat does not exist");

            var messages = await _database.MessageRepository.GetMessagesAsync(chatId, ct);
            var filteredMessages = messages.Where(m => m.IsDeleted == false).ToList();
            
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
                    Sender: sender == null ? null : new UserShortDTO(
                        Id: sender.Id,
                        Username: sender.Username,
                        Email: sender.Email,
                        DepartmentName: sender.Department?.Name ?? "No department"),
                    RepliedMessage: repliedMessage == null ? null : new MessageDTO(
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

            return Result<List<MessageDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<MessageDTO>>.Failure($"Failed to get messages: {e.Message}");
        }
    }

    public async Task<Result<MessageDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var message = await _database.MessageRepository.GetByIdAsync(id, ct);
            if (message == null)
                return Result<MessageDTO>.Failure("Message not found");

            if (message.IsDeleted)
                return Result<MessageDTO>.Failure("Message is deleted");
            
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
                        Sender: repliedMessageSender == null ? null : new UserShortDTO(
                            Id: repliedMessageSender.Id,
                            Username: repliedMessageSender.Username,
                            Email: repliedMessageSender.Email,
                            DepartmentName: repliedMessageSender.Department?.Name ?? "No department"),
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
                Sender: sender == null ? null : new UserShortDTO(
                    Id: sender.Id,
                    Username: sender.Username,
                    Email: sender.Email,
                    DepartmentName: sender.Department?.Name ?? "No department"),
                RepliedMessage: repliedMessageDto,
                Replies: new List<MessageDTO>(),
                IsOwn: sender?.Id == message.SenderId
                );

            return Result<MessageDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<MessageDTO>.Failure($"Failed to get message: {e.Message}");
        }
    }
}