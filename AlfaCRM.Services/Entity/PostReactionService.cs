using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class PostReactionService : IPostReactionService
{
    private readonly IUnitOfWork _database;

    public PostReactionService(IUnitOfWork database)
    {
        _database = database;
    }
    
    public async Task<bool> Create(PostReactionCreateRequest request)
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

            return result > 0;
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<bool> Delete(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var dbReaction = await _database.PostReactionRepository.GetByIdAsync(id);
            if (dbReaction == null)
            {
                throw new KeyNotFoundException();
            }

            _database.PostReactionRepository.Delete(dbReaction);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);

            return result > 0;
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}