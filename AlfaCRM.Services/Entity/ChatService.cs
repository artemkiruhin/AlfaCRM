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
            if (user == null) return false;
        }
        return true;
    }
    
    public async Task<Result<Guid>> Create(ChatCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            if (request.IsPersonal && request.MembersIds.Count != 1)
                return Result<Guid>.Failure("Personal chat must have 1 member!");

            var membersIdsIncludeCreator = new List<Guid>(request.MembersIds) { request.Creator };
            
            var usersExist = await AllUsersExist(membersIdsIncludeCreator, ct);
            if (!usersExist) return Result<Guid>.Failure("At least one user is not exist!");
            
            var newChat = ChatEntity.Create(request.Name, request.Creator);
            await _database.ChatRepository.CreateAsync(newChat, ct);
            await _database.SaveChangesAsync(ct);
           
            var chat = await _database.ChatRepository.GetByIdAsync(newChat.Id, ct);
            if (chat == null) return Result<Guid>.Failure("Chat not created!");
            
            await AddMembers(chat.Id, membersIdsIncludeCreator, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            return Result<Guid>.Success(newChat.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while creating chat: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(ChatUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            if (string.IsNullOrEmpty(request.Name)) 
                return Result<Guid>.Failure("Name is required!");
            
            var chatExist = await _database.ChatRepository.GetByIdAsync(request.Id, ct);
            if (chatExist == null) 
                return Result<Guid>.Failure("Chat doesn't exist!");
            
            chatExist.Name = request.Name;
            _database.ChatRepository.Update(chatExist, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            return Result<Guid>.Success(chatExist.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while updating chat: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var dbChat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (dbChat == null) 
                return Result<Guid>.Failure("Chat not found");
            
            _database.ChatRepository.Delete(dbChat, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0 
                ? Result<Guid>.Success(dbChat.Id) 
                : Result<Guid>.Failure("Failed to delete chat");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while deleting chat: {ex.Message}");
        }
    }

    public async Task<Result<List<Guid>>> AddMembers(Guid chatId, List<Guid> memberIds, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null) 
                return Result<List<Guid>>.Failure("Chat not found");

            var membersExist = await AllUsersExist(memberIds, ct);
            if (!membersExist) 
                return Result<List<Guid>>.Failure("At least one user is not exist!");
            
            foreach (var member in memberIds) 
                await PartAddMember(chat, member, ct);
            
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            return Result<List<Guid>>.Success(memberIds);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<List<Guid>>.Failure($"Error while adding members: {e.Message}");
        }
    }

    private async Task<Result<Guid>> PartAddMember(ChatEntity chat, Guid memberId, CancellationToken ct)
    {
        var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
        if (member == null) 
            return Result<Guid>.Failure("User not found!");

        if (chat.Members.Any(m => m.Id == memberId))
            return Result<Guid>.Failure("User is already a member of this chat");

        chat.Members.Add(member);
        _database.ChatRepository.Update(chat, ct);
        return Result<Guid>.Success(member.Id);
    }

    public async Task<Result<Guid>> AddMember(Guid chatId, Guid memberId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null) 
                return Result<Guid>.Failure("Chat not found");
            
            var result = await PartAddMember(chat, memberId, ct);
            if (!result.IsSuccess)
                return result;
                
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            return result;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while adding member: {e.Message}");
        }
    }
    
    public async Task<Result<List<Guid>>> RemoveMember(Guid chatId, Guid memberId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var chat = await _database.ChatRepository.GetByIdAsync(chatId, ct);
            if (chat == null) 
                return Result<List<Guid>>.Failure("Chat not found");
            
            var member = await _database.UserRepository.GetByIdAsync(memberId, ct);
            if (member == null) 
                return Result<List<Guid>>.Failure("User not found!");
            
            if (chat.Admin?.Id == memberId)
                return Result<List<Guid>>.Failure("Cannot remove chat admin");
            
            var memberToRemove = chat.Members.FirstOrDefault(m => m.Id == memberId);
            if (memberToRemove == null)
                return Result<List<Guid>>.Failure("User is not a member of this chat");
            
            chat.Members.Remove(memberToRemove);
            _database.ChatRepository.Update(chat, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            return Result<List<Guid>>.Success(chat.Members.Select(m => m.Id).ToList());
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<List<Guid>>.Failure($"Error while removing member: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetByNameAsync(string name, CancellationToken ct)
    {
        try
        {
            var chats = await _database.ChatRepository.GetByNameAsync(name, ct);
            var dtos = chats.Select(entity => new ChatShortDTO(
                Id: entity.Id,
                Name: entity.Name,
                CreatedAt: entity.CreatedAt,
                AdminId: entity.Admin?.Id
            )).ToList();
            
            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<ChatShortDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetByUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(userId, ct);
            if (user == null) 
                return Result<List<ChatShortDTO>>.Failure("User not found");
            
            var chats = await _database.ChatRepository.GetByUserAsync(userId, ct);
            var dtos = chats.Select(entity => new ChatShortDTO(
                Id: entity.Id,
                Name: entity.Name,
                CreatedAt: entity.CreatedAt,
                AdminId: entity.Admin?.Id
            )).ToList();
            
            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<ChatShortDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<List<ChatShortDTO>>> GetAllShort(CancellationToken ct)
    {
        try
        {
            var chats = await _database.ChatRepository.GetAllAsync(ct);
            var dtos = chats.Select(entity => new ChatShortDTO(
                Id: entity.Id,
                Name: entity.Name,
                CreatedAt: entity.CreatedAt,
                AdminId: entity.Admin?.Id
            )).ToList();
            
            return Result<List<ChatShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<ChatShortDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<List<ChatDetailedDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            var chats = await _database.ChatRepository.GetAllAsync(ct);
            var dtos = new List<ChatDetailedDTO>();

            foreach (var chat in chats)
            {
                var dto = new ChatDetailedDTO(
                    Id: chat.Id,
                    Name: chat.Name,
                    CreatedAt: chat.CreatedAt,
                    Admin: chat.Admin == null ? null : new UserShortDTO(
                        Id: chat.Admin.Id,
                        Username: chat.Admin.Username,
                        Email: chat.Admin.Email,
                        DepartmentName: chat.Admin.Department?.Name ?? "Нет отдела"
                    ),
                    Messages: chat.Messages
                        .Where(m => !m.IsDeleted)
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(50)
                        .Select(message => new MessageDTO(
                            Id: message.Id,
                            Content: message.Content,
                            CreatedAt: message.CreatedAt,
                            UpdatedAt: message.UpdatedAt,
                            DeletedAt: message.DeletedAt,
                            IsDeleted: message.IsDeleted,
                            IsPinned: message.IsPinned,
                            Sender: new UserShortDTO(
                                Id: message.Sender.Id,
                                Username: message.Sender.Username,
                                Email: message.Sender.Email,
                                DepartmentName: message.Sender.Department?.Name ?? "Нет отдела"
                            ),
                            RepliedMessage: message.RepliedMessage == null ? null : new MessageDTO(
                                Id: message.RepliedMessage.Id,
                                Content: message.RepliedMessage.Content,
                                CreatedAt: message.RepliedMessage.CreatedAt,
                                UpdatedAt: message.RepliedMessage.UpdatedAt,
                                DeletedAt: message.RepliedMessage.DeletedAt,
                                IsDeleted: message.RepliedMessage.IsDeleted,
                                IsPinned: message.RepliedMessage.IsPinned,
                                Sender: new UserShortDTO(
                                    Id: message.RepliedMessage.Sender.Id,
                                    Username: message.RepliedMessage.Sender.Username,
                                    Email: message.RepliedMessage.Sender.Email,
                                    DepartmentName: message.RepliedMessage.Sender.Department?.Name ?? "Нет отдела"
                                ),
                                RepliedMessage: null,
                                Replies: new List<MessageDTO>()
                            ),
                            Replies: new List<MessageDTO>()
                        )).ToList(),
                    Members: chat.Members.Select(member => new UserShortDTO(
                        Id: member.Id,
                        Username: member.Username,
                        Email: member.Email,
                        DepartmentName: member.Department?.Name ?? "Нет отдела"
                    )).ToList()
                );
                
                dtos.Add(dto);
            }
            
            return Result<List<ChatDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<ChatDetailedDTO>>.Failure($"Error while getting chats: {e.Message}");
        }
    }

    public async Task<Result<ChatDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var chat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (chat == null)
                return Result<ChatDetailedDTO>.Failure("Chat not found");

            var dto = new ChatDetailedDTO(
                Id: chat.Id,
                Name: chat.Name,
                CreatedAt: chat.CreatedAt,
                Admin: chat.Admin == null ? null : new UserShortDTO(
                    Id: chat.Admin.Id,
                    Username: chat.Admin.Username,
                    Email: chat.Admin.Email,
                    DepartmentName: chat.Admin.Department?.Name ?? "Нет отдела"
                ),
                Messages: chat.Messages
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(100)
                    .Select(message => new MessageDTO(
                        Id: message.Id,
                        Content: message.Content,
                        CreatedAt: message.CreatedAt,
                        UpdatedAt: message.UpdatedAt,
                        DeletedAt: message.DeletedAt,
                        IsDeleted: message.IsDeleted,
                        IsPinned: message.IsPinned,
                        Sender: new UserShortDTO(
                            Id: message.Sender.Id,
                            Username: message.Sender.Username,
                            Email: message.Sender.Email,
                            DepartmentName: message.Sender.Department?.Name ?? "Нет отдела"
                        ),
                        RepliedMessage: message.RepliedMessage == null ? null : new MessageDTO(
                            Id: message.RepliedMessage.Id,
                            Content: message.RepliedMessage.Content,
                            CreatedAt: message.RepliedMessage.CreatedAt,
                            UpdatedAt: message.RepliedMessage.UpdatedAt,
                            DeletedAt: message.RepliedMessage.DeletedAt,
                            IsDeleted: message.RepliedMessage.IsDeleted,
                            IsPinned: message.RepliedMessage.IsPinned,
                            Sender: new UserShortDTO(
                                Id: message.RepliedMessage.Sender.Id,
                                Username: message.RepliedMessage.Sender.Username,
                                Email: message.RepliedMessage.Sender.Email,
                                DepartmentName: message.RepliedMessage.Sender.Department?.Name ?? "Нет отдела"
                            ),
                            RepliedMessage: null,
                            Replies: new List<MessageDTO>()
                        ),
                        Replies: new List<MessageDTO>()
                    )).ToList(),
                Members: chat.Members.Select(member => new UserShortDTO(
                    Id: member.Id,
                    Username: member.Username,
                    Email: member.Email,
                    DepartmentName: member.Department?.Name ?? "Нет отдела"
                )).ToList()
            );
            
            return Result<ChatDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<ChatDetailedDTO>.Failure($"Error while getting chat: {e.Message}");
        }
    }

    public async Task<Result<ChatShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            var chat = await _database.ChatRepository.GetByIdAsync(id, ct);
            if (chat == null)
                return Result<ChatShortDTO>.Failure("Chat not found");

            var dto = new ChatShortDTO(
                Id: chat.Id,
                Name: chat.Name,
                CreatedAt: chat.CreatedAt,
                AdminId: chat.Admin?.Id
            );
            
            return Result<ChatShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<ChatShortDTO>.Failure($"Error while getting chat: {e.Message}");
        }
    }
}