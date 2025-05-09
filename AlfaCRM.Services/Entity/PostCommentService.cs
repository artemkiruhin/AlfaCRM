using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure;

namespace AlfaCRM.Services.Entity;

public class PostCommentService : IPostCommentService
{
    private readonly IUnitOfWork _database;

    public PostCommentService(IUnitOfWork database)
    {
        _database = database;
    }

    private PostCommentShortDTO MapShort(PostCommentEntity entity)
    {
        return new PostCommentShortDTO(
            Id: entity.Id,
            Content: entity.Content,
            IsDeleted: entity.IsDeleted,
            CreatedAt: entity.CreatedAt,
            Sender: new UserShortDTO(
                Id: entity.SenderId,
                FullName: entity.Sender.FullName,
                Username: entity.Sender.Username,
                Email: entity.Sender.Email,
                DepartmentName: entity.Sender.Department?.Name ?? "Нет отдела",
                IsAdmin: entity.Sender?.IsAdmin ?? false,
                IsBlocked: entity.Sender?.IsBlocked ?? false
            )
        );
    }

    private List<PostCommentShortDTO> MapShortRange(IEnumerable<PostCommentEntity> entities)
    {
        return entities.Select(entity => new PostCommentShortDTO(
            Id: entity.Id,
            Content: entity.Content,
            IsDeleted: entity.IsDeleted,
            CreatedAt: entity.CreatedAt,
            Sender: new UserShortDTO(
                Id: entity.SenderId,
                FullName: entity.Sender.FullName,
                Username: entity.Sender.Username,
                Email: entity.Sender.Email,
                DepartmentName: entity.Sender.Department?.Name ?? "Нет отдела",
                IsAdmin: entity.Sender?.IsAdmin ?? false,
                IsBlocked: entity.Sender?.IsBlocked ?? false
            )
        )).ToList();
    }

    public async Task<Result<Guid>> Create(PostCommentCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting post comment creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            // Validate post exists
            var postExists = await _database.PostRepository.FindAsync(p => p.Id == request.PostId, ct);
            if (postExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create comment: post with ID {request.PostId} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Post not found");
            }

            // Validate sender exists
            var senderExists = await _database.UserRepository.FindAsync(u => u.Id == request.SenderId, ct);
            if (senderExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create comment: sender with ID {request.SenderId} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Sender not found");
            }

            var newComment = PostCommentEntity.Create(
                content: request.Content,
                postId: request.PostId,
                senderId: request.SenderId
            );
            
            await _database.PostCommentRepository.CreateAsync(newComment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Post comment created successfully with ID: {newComment.Id} for post {request.PostId}",
                    request.SenderId), ct);
                return Result<Guid>.Success(newComment.Id);
            }
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "No changes were made when creating post comment",
                request.SenderId), ct);
            return Result<Guid>.Failure("Failed to create comment");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating post comment: {ex.Message}. StackTrace: {ex.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Error while creating comment: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting post comment deletion process for ID {id}",
                null), ct);

            var dbComment = await _database.PostCommentRepository.GetByIdAsync(id, ct);
            if (dbComment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete comment: comment with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("Comment not found");
            }

            _database.PostCommentRepository.Delete(dbComment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Post comment {id} deleted successfully",
                    null), ct);
                return Result<Guid>.Success(dbComment.Id);
            }
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when deleting post comment {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to delete comment");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting post comment {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while deleting comment: {ex.Message}");
        }
    }

    public async Task<Result<List<PostCommentShortDTO>>> GetAll(Guid postId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve all comments for post {postId}",
                null), ct);

            // Validate post exists
            var postExists = await _database.PostRepository.FindAsync(p => p.Id == postId, ct);
            if (postExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get comments: post with ID {postId} not found",
                    null), ct);
                return Result<List<PostCommentShortDTO>>.Failure("Post not found");
            }

            var comments = await _database.PostCommentRepository.FindRangeAsync(
                comment => comment.PostId == postId, 
                ct);
            
            var dtos = MapShortRange(comments);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} comments for post {postId}",
                null), ct);
            
            return Result<List<PostCommentShortDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving comments for post {postId}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<List<PostCommentShortDTO>>.Failure($"Error while retrieving comments: {ex.Message}");
        }
    }

    public async Task<Result<PostCommentShortDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve post comment with ID {id}",
                null), ct);

            var comment = await _database.PostCommentRepository.GetByIdAsync(id, ct);
            if (comment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get comment: comment with ID {id} not found",
                    null), ct);
                return Result<PostCommentShortDTO>.Failure("Comment not found");
            }

            var dto = MapShort(comment);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved post comment with ID {id}",
                null), ct);
            
            return Result<PostCommentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving post comment {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<PostCommentShortDTO>.Failure($"Error while retrieving comment: {ex.Message}");
        }
    }
}