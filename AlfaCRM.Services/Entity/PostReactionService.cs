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
    
    public async Task<Result<Guid>> Create(PostReactionCreateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var existingReactions = await _database.PostReactionRepository
                .FindRangeAsync(reaction => reaction.PostId == request.PostId);
            
            var reactionsToDelete = existingReactions
                .Where(reaction => reaction.Type != request.Type)
                .ToList();

            foreach (var reaction in reactionsToDelete)
            {
                _database.PostReactionRepository.Delete(reaction);
            }
            
            if (!existingReactions.Any())
            {
                var newReaction = PostReactionEntity.Create(
                    postId: request.PostId,
                    senderId: request.SenderId,
                    type: request.Type
                );

                await _database.PostReactionRepository.CreateAsync(newReaction);
            }
            
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

    public async Task<Result<Guid>> Delete(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var dbReaction = await _database.PostReactionRepository.GetByIdAsync(id);
            if (dbReaction == null)
            {
                return Result<Guid>.Failure("Reaction not found");
            }

            _database.PostReactionRepository.Delete(dbReaction);
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
}