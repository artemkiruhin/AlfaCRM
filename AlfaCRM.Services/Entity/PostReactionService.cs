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
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var existingReactions = await _database.PostReactionRepository
                .FindRangeAsync(reaction => reaction.PostId == request.PostId, ct);

            foreach (var reaction in existingReactions) _database.PostReactionRepository.Delete(reaction, ct);
            
            await _database.SaveChangesAsync(ct);
            
            var newReaction = PostReactionEntity.Create(
                postId: request.PostId,
                senderId: request.SenderId,
                type: request.Type
            );

            await _database.PostReactionRepository.CreateAsync(newReaction, ct);
            
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);

            return result > 0
                ? Result<Guid>.Success(request.PostId)
                : Result<Guid>.Failure("Failed to create reaction");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while creating reaction: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var dbReaction = await _database.PostReactionRepository.GetByIdAsync(id, ct);
            if (dbReaction == null)
            {
                return Result<Guid>.Failure("Reaction not found");
            }

            _database.PostReactionRepository.Delete(dbReaction, ct);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);

            return result > 0 
                ? Result<Guid>.Success(dbReaction.Id) 
                : Result<Guid>.Failure("Failed to delete reaction");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while deleting reaction: {ex.Message}");
        }
    }
    
    public async Task<Result<bool>> DeleteAll(Guid postId, Guid userId, CancellationToken ct){
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var post = await _database.PostRepository.GetByIdAsync(postId, ct);
            if (post == null)
            {
                return Result<bool>.Failure("Post not found");
            }

            post.Reactions.Where(reaction => reaction.SenderId == userId).ToList().ForEach(reaction => _database.PostReactionRepository.Delete(reaction, ct));
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);

            return result > 0 
                ? Result<bool>.Success(true) 
                : Result<bool>.Failure("Failed to delete reaction");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<bool>.Failure($"Error while deleting reaction: {ex.Message}");
        }
    }
}