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
                $"Начало процесса создания комментария к посту. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            // Проверка существования поста
            var postExists = await _database.PostRepository.FindAsync(p => p.Id == request.PostId, ct);
            if (postExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать комментарий: пост с ID {request.PostId} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Пост не найден");
            }

            // Проверка существования отправителя
            var senderExists = await _database.UserRepository.FindAsync(u => u.Id == request.SenderId, ct);
            if (senderExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать комментарий: отправитель с ID {request.SenderId} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Отправитель не найден");
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
                    $"Комментарий к посту успешно создан с ID: {newComment.Id} для поста {request.PostId}",
                    request.SenderId), ct);
                return Result<Guid>.Success(newComment.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "Не было внесено изменений при создании комментария",
                request.SenderId), ct);
            return Result<Guid>.Failure("Не удалось создать комментарий");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании комментария: {ex.Message}. StackTrace: {ex.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Ошибка при создании комментария: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления комментария с ID {id}",
                null), ct);

            var dbComment = await _database.PostCommentRepository.GetByIdAsync(id, ct);
            if (dbComment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить комментарий: комментарий с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Комментарий не найден");
            }

            _database.PostCommentRepository.Delete(dbComment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Комментарий {id} успешно удален",
                    null), ct);
                return Result<Guid>.Success(dbComment.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при удалении комментария {id}",
                null), ct);
            return Result<Guid>.Failure("Не удалось удалить комментарий");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении комментария {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при удалении комментария: {ex.Message}");
        }
    }

    public async Task<Result<List<PostCommentShortDTO>>> GetAll(Guid postId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения всех комментариев для поста {postId}",
                null), ct);

            // Проверка существования поста
            var postExists = await _database.PostRepository.FindAsync(p => p.Id == postId, ct);
            if (postExists == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить комментарии: пост с ID {postId} не найден",
                    null), ct);
                return Result<List<PostCommentShortDTO>>.Failure("Пост не найден");
            }

            var comments = await _database.PostCommentRepository.FindRangeAsync(
                comment => comment.PostId == postId,
                ct);

            var dtos = MapShortRange(comments);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} комментариев для поста {postId}",
                null), ct);

            return Result<List<PostCommentShortDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении комментариев для поста {postId}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<List<PostCommentShortDTO>>.Failure($"Ошибка при получении комментариев: {ex.Message}");
        }
    }

    public async Task<Result<PostCommentShortDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения комментария с ID {id}",
                null), ct);

            var comment = await _database.PostCommentRepository.GetByIdAsync(id, ct);
            if (comment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить комментарий: комментарий с ID {id} не найден",
                    null), ct);
                return Result<PostCommentShortDTO>.Failure("Комментарий не найден");
            }

            var dto = MapShort(comment);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Комментарий с ID {id} успешно получен",
                null), ct);

            return Result<PostCommentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении комментария {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<PostCommentShortDTO>.Failure($"Ошибка при получении комментария: {ex.Message}");
        }
    }
}