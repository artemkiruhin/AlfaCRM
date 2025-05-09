using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class PostReactionService : IPostReactionService
{
    private readonly IUnitOfWork _database;

    public PostReactionService(IUnitOfWork database)
    {
        _database = database;
    }
    
    public async Task<Result<Guid>> Create(PostReactionCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting post reaction creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            // Validate post exists
            var post = await _database.PostRepository.GetByIdAsync(request.PostId, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create reaction: post with ID {request.PostId} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Post not found");
            }

            // Validate sender exists
            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create reaction: sender with ID {request.SenderId} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Sender not found");
            }

            // Remove existing reactions from this user for this post
            var existingReactions = await _database.PostReactionRepository
                .FindRangeAsync(reaction => 
                    reaction.PostId == request.PostId && 
                    reaction.SenderId == request.SenderId, 
                ct);

            if (existingReactions.Any())
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Removing {existingReactions.Count()} existing reactions for post {request.PostId} from user {request.SenderId}",
                    request.SenderId), ct);

                foreach (var reaction in existingReactions)
                {
                    _database.PostReactionRepository.Delete(reaction, ct);
                }
                await _database.SaveChangesAsync(ct);
            }

            // Create new reaction
            var newReaction = PostReactionEntity.Create(
                postId: request.PostId,
                senderId: request.SenderId,
                type: request.Type
            );

            await _database.PostReactionRepository.CreateAsync(newReaction, ct);
            
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Reaction created successfully for post {request.PostId} by user {request.SenderId}. Type: {request.Type}",
                    request.SenderId), ct);
                return Result<Guid>.Success(request.PostId);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "No changes were made when creating reaction",
                request.SenderId), ct);
            return Result<Guid>.Failure("Failed to create reaction");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating reaction: {ex.Message}. StackTrace: {ex.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Error while creating reaction: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting reaction deletion process for ID {id}",
                null), ct);

            var dbReaction = await _database.PostReactionRepository.GetByIdAsync(id, ct);
            if (dbReaction == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete reaction: reaction with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("Reaction not found");
            }

            _database.PostReactionRepository.Delete(dbReaction, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Reaction {id} deleted successfully from post {dbReaction.PostId}",
                    null), ct);
                return Result<Guid>.Success(dbReaction.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when deleting reaction {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to delete reaction");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting reaction {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while deleting reaction: {ex.Message}");
        }
    }
    
    public async Task<Result<bool>> DeleteAll(Guid postId, Guid userId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to delete all reactions for post {postId} from user {userId}",
                userId), ct);

            var post = await _database.PostRepository.GetByIdAsync(postId, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete reactions: post with ID {postId} not found",
                    userId), ct);
                return Result<bool>.Failure("Post not found");
            }

            var userReactions = post.Reactions
                .Where(reaction => reaction.SenderId == userId)
                .ToList();

            if (!userReactions.Any())
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"No reactions found for post {postId} from user {userId} to delete",
                    userId), ct);
                return Result<bool>.Failure("No reactions found to delete");
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Deleting {userReactions.Count} reactions for post {postId} from user {userId}",
                userId), ct);

            foreach (var reaction in userReactions)
            {
                _database.PostReactionRepository.Delete(reaction, ct);
            }

            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Successfully deleted {userReactions.Count} reactions for post {postId} from user {userId}",
                    userId), ct);
                return Result<bool>.Success(true);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when deleting reactions for post {postId} from user {userId}",
                userId), ct);
            return Result<bool>.Failure("Failed to delete reactions");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting reactions for post {postId} from user {userId}: {ex.Message}. StackTrace: {ex.StackTrace}",
                userId), ct);
            return Result<bool>.Failure($"Error while deleting reactions: {ex.Message}");
        }
    }
}