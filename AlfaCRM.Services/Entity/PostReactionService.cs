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
                $"Начало процесса создания реакции на пост. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            // Проверка существования поста
            var post = await _database.PostRepository.GetByIdAsync(request.PostId, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать реакцию: пост с ID {request.PostId} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Пост не найден");
            }

            // Проверка существования отправителя
            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать реакцию: отправитель с ID {request.SenderId} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Отправитель не найден");
            }

            // Удаление существующих реакций этого пользователя для данного поста
            var existingReactions = await _database.PostReactionRepository
                .FindRangeAsync(reaction =>
                        reaction.PostId == request.PostId &&
                        reaction.SenderId == request.SenderId,
                    ct);

            if (existingReactions.Any())
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Удаление {existingReactions.Count()} существующих реакций для поста {request.PostId} от пользователя {request.SenderId}",
                    request.SenderId), ct);

                foreach (var reaction in existingReactions)
                {
                    _database.PostReactionRepository.Delete(reaction, ct);
                }

                await _database.SaveChangesAsync(ct);
            }

            // Создание новой реакции
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
                    $"Реакция успешно создана для поста {request.PostId} пользователем {request.SenderId}. Тип: {request.Type}",
                    request.SenderId), ct);
                return Result<Guid>.Success(request.PostId);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "Не было внесено изменений при создании реакции",
                request.SenderId), ct);
            return Result<Guid>.Failure("Не удалось создать реакцию");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании реакции: {ex.Message}. StackTrace: {ex.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Ошибка при создании реакции: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления реакции с ID {id}",
                null), ct);

            var dbReaction = await _database.PostReactionRepository.GetByIdAsync(id, ct);
            if (dbReaction == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить реакцию: реакция с ID {id} не найдена",
                    null), ct);
                return Result<Guid>.Failure("Реакция не найдена");
            }

            _database.PostReactionRepository.Delete(dbReaction, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Реакция {id} успешно удалена с поста {dbReaction.PostId}",
                    null), ct);
                return Result<Guid>.Success(dbReaction.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при удалении реакции {id}",
                null), ct);
            return Result<Guid>.Failure("Не удалось удалить реакцию");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении реакции {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при удалении реакции: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAll(Guid postId, Guid userId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало удаления всех реакций для поста {postId} от пользователя {userId}",
                userId), ct);

            var post = await _database.PostRepository.GetByIdAsync(postId, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить реакции: пост с ID {postId} не найден",
                    userId), ct);
                return Result<bool>.Failure("Пост не найден");
            }

            var userReactions = post.Reactions
                .Where(reaction => reaction.SenderId == userId)
                .ToList();

            if (!userReactions.Any())
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не найдено реакций для поста {postId} от пользователя {userId} для удаления",
                    userId), ct);
                return Result<bool>.Failure("Не найдено реакций для удаления");
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Удаление {userReactions.Count} реакций для поста {postId} от пользователя {userId}",
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
                    $"Успешно удалено {userReactions.Count} реакций для поста {postId} от пользователя {userId}",
                    userId), ct);
                return Result<bool>.Success(true);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при удалении реакций для поста {postId} от пользователя {userId}",
                userId), ct);
            return Result<bool>.Failure("Не удалось удалить реакции");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении реакций для поста {postId} от пользователя {userId}: {ex.Message}. StackTrace: {ex.StackTrace}",
                userId), ct);
            return Result<bool>.Failure($"Ошибка при удалении реакций: {ex.Message}");
        }
    }
}