using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class PostService : IPostService
{
    private readonly IUnitOfWork _database;

    public PostService(IUnitOfWork database)
    {
        _database = database;
    }

    private string ReactionTypeToString(ReactionType reactionType)
    {
        return reactionType switch
        {
            ReactionType.Like => "Like",
            ReactionType.Dislike => "Dislike",
            _ => "Unknown"
        };
    }

    private PostShortDTO MapToShortDTO(PostEntity entity)
    {
        return new PostShortDTO(
            Id: entity.Id,
            Title: entity.Title,
            CreatedAt: entity.CreatedAt,
            IsImportant: entity.IsImportant,
            Department: entity.Department?.Name ?? "Общая новость",
            DepartmentId: entity.DepartmentId
        );
    }

    private List<PostShortDTO> MapToShortDTORange(IEnumerable<PostEntity> entities)
    {
        return entities.Select(entity => new PostShortDTO(
            Id: entity.Id,
            Title: entity.Title,
            CreatedAt: entity.CreatedAt,
            IsImportant: entity.IsImportant,
            Department: entity.Department?.Name ?? "Общая новость",
            DepartmentId: entity.DepartmentId
        )).ToList();
    }

    private PostDetailedDTO MapToDetailedDTO(PostEntity entity)
    {
        return new PostDetailedDTO(
            Id: entity.Id,
            Title: entity.Title,
            Subtitle: entity.Subtitle,
            Content: entity.Content,
            CreatedAt: entity.CreatedAt,
            ModifiedAt: entity.ModifiedAt,
            IsImportant: entity.IsImportant,
            IsActual: entity.IsActual,
            Publisher: new UserShortDTO(
                Id: entity.PublisherId,
                FullName: entity.Publisher.FullName,
                Username: entity.Publisher.Username,
                Email: entity.Publisher.Email,
                DepartmentName: entity.Publisher.Department?.Name ?? "Нет отдела",
                IsAdmin: entity.Publisher?.IsAdmin ?? false,
                IsBlocked: entity.Publisher?.IsBlocked ?? false
            ),
            Department: entity.DepartmentId.HasValue
                ? new DepartmentShortDTO(
                    Id: entity.DepartmentId.Value,
                    Name: entity.Department.Name,
                    MembersCount: entity.Department.Users.Count,
                    IsSpecific: entity.Department.IsSpecific
                )
                : null,
            Reactions: entity.Reactions.Select(reaction => new PostReactionShortDTO(
                Id: reaction.Id,
                Sender: new UserShortDTO(
                    Id: reaction.SenderId,
                    FullName: reaction.Sender.FullName,
                    Username: reaction.Sender.Username,
                    Email: reaction.Sender.Email,
                    DepartmentName: reaction.Sender.Department?.Name ?? "Нет отдела",
                    IsAdmin: reaction.Sender?.IsAdmin ?? false,
                    IsBlocked: reaction.Sender?.IsBlocked ?? false
                ),
                CreatedAt: reaction.CreatedAt,
                Type: ReactionTypeToString(reaction.Type)
            )).ToList(),
            Comments: entity.Comments.Select(comment => new PostCommentShortDTO(
                Id: comment.Id,
                Content: comment.Content,
                IsDeleted: comment.IsDeleted,
                CreatedAt: comment.CreatedAt,
                Sender: new UserShortDTO(
                    Id: comment.SenderId,
                    FullName: comment.Sender.FullName,
                    Username: comment.Sender.Username,
                    Email: comment.Sender.Email,
                    DepartmentName: comment.Sender.Department?.Name ?? "Нет отдела",
                    IsAdmin: comment.Sender?.IsAdmin ?? false,
                    IsBlocked: comment.Sender?.IsBlocked ?? false
                )
            )).ToList()
        );
    }

    private List<PostDetailedDTO> MapToDetailedDTORange(IEnumerable<PostEntity> entities)
    {
        return entities.Select(entity => new PostDetailedDTO(
            Id: entity.Id,
            Title: entity.Title,
            Subtitle: entity.Subtitle,
            Content: entity.Content,
            CreatedAt: entity.CreatedAt,
            ModifiedAt: entity.ModifiedAt,
            IsImportant: entity.IsImportant,
            IsActual: entity.IsActual,
            Publisher: new UserShortDTO(
                Id: entity.PublisherId,
                FullName: entity.Publisher.FullName,
                Username: entity.Publisher.Username,
                Email: entity.Publisher.Email,
                DepartmentName: entity.Publisher.Department?.Name ?? "Нет отдела",
                IsAdmin: entity.Publisher?.IsAdmin ?? false,
                IsBlocked: entity.Publisher?.IsBlocked ?? false
            ),
            Department: entity.DepartmentId.HasValue
                ? new DepartmentShortDTO(
                    Id: entity.DepartmentId.Value,
                    Name: entity.Department.Name,
                    MembersCount: entity.Department.Users.Count,
                    IsSpecific: entity.Department.IsSpecific
                )
                : null,
            Reactions: entity.Reactions.Select(reaction => new PostReactionShortDTO(
                Id: reaction.Id,
                Sender: new UserShortDTO(
                    Id: reaction.SenderId,
                    FullName: reaction.Sender.FullName,
                    Username: reaction.Sender.Username,
                    Email: reaction.Sender.Email,
                    DepartmentName: reaction.Sender.Department?.Name ?? "Нет отдела",
                    IsAdmin: reaction.Sender?.IsAdmin ?? false,
                    IsBlocked: reaction.Sender?.IsBlocked ?? false
                ),
                CreatedAt: reaction.CreatedAt,
                Type: ReactionTypeToString(reaction.Type)
            )).ToList(),
            Comments: entity.Comments.Select(comment => new PostCommentShortDTO(
                Id: comment.Id,
                Content: comment.Content,
                IsDeleted: comment.IsDeleted,
                CreatedAt: comment.CreatedAt,
                Sender: new UserShortDTO(
                    Id: comment.SenderId,
                    FullName: comment.Sender.FullName,
                    Username: comment.Sender.Username,
                    Email: comment.Sender.Email,
                    DepartmentName: comment.Sender.Department?.Name ?? "Нет отдела",
                    IsAdmin: comment.Sender?.IsAdmin ?? false,
                    IsBlocked: comment.Sender?.IsBlocked ?? false
                )
            )).ToList()
        )).ToList();
    }

    public async Task<Result<Guid>> Create(PostCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса создания поста. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.PublisherId), ct);

            // Проверка существования автора
            var publisher = await _database.UserRepository.GetByIdAsync(request.PublisherId, ct);
            if (publisher == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать пост: автор с ID {request.PublisherId} не найден",
                    request.PublisherId), ct);
                return Result<Guid>.Failure("Автор не найден");
            }

            // Проверка существования отдела, если указан
            if (request.DepartmentId.HasValue)
            {
                var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value, ct);
                if (department == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Не удалось создать пост: отдел с ID {request.DepartmentId} не найден",
                        request.PublisherId), ct);
                    return Result<Guid>.Failure("Отдел не найден");
                }
            }

            var newPost = PostEntity.Create(
                title: request.Title,
                subtitle: request.Subtitle,
                content: request.Content,
                isImportant: request.IsImportant,
                departmentId: request.DepartmentId,
                publisherId: request.PublisherId
            );

            await _database.PostRepository.CreateAsync(newPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пост успешно создан с ID: {newPost.Id}",
                    request.PublisherId), ct);
                return Result<Guid>.Success(newPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "Не было внесено изменений при создании поста",
                request.PublisherId), ct);
            return Result<Guid>.Failure("Не удалось создать пост");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании поста: {e.Message}. StackTrace: {e.StackTrace}",
                request?.PublisherId), ct);
            return Result<Guid>.Failure($"Ошибка при создании поста: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(PostUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса обновления поста с ID {request.PostId}. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var dbPost = await _database.PostRepository.GetByIdAsync(request.PostId, ct);
            if (dbPost == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить пост: пост с ID {request.PostId} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пост не найден");
            }

            // Логирование изменений
            if (!string.IsNullOrEmpty(request.Title) && dbPost.Title != request.Title)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление заголовка поста {request.PostId} с '{dbPost.Title}' на '{request.Title}'",
                    null), ct);
                dbPost.Title = request.Title;
            }

            if (!string.IsNullOrEmpty(request.Subtitle) && dbPost.Subtitle != request.Subtitle)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление подзаголовка поста {request.PostId} с '{dbPost.Subtitle}' на '{request.Subtitle}'",
                    null), ct);
                dbPost.Subtitle = request.Subtitle;
            }

            if (!string.IsNullOrEmpty(request.Content) && dbPost.Content != request.Content)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление содержимого поста {request.PostId}",
                    null), ct);
                dbPost.Content = request.Content;
            }

            if (request.IsImportant.HasValue && dbPost.IsImportant != request.IsImportant.Value)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление важности поста {request.PostId} с {dbPost.IsImportant} на {request.IsImportant.Value}",
                    null), ct);
                dbPost.IsImportant = request.IsImportant.Value;
            }

            if (request.DepartmentId.HasValue || request.EditDepartment)
            {
                if (request.DepartmentId.HasValue)
                {
                    var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value, ct);
                    if (department == null)
                    {
                        await _database.LogRepository.CreateAsync(LogEntity.Create(
                            LogType.Warning,
                            $"Не удалось обновить пост: отдел с ID {request.DepartmentId} не найден",
                            null), ct);
                        return Result<Guid>.Failure("Отдел не найден");
                    }
                }

                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление отдела поста {request.PostId} с {dbPost.DepartmentId} на {request.DepartmentId}",
                    null), ct);
                dbPost.DepartmentId = request.DepartmentId;
            }

            dbPost.ModifiedAt = DateTime.UtcNow;

            _database.PostRepository.Update(dbPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пост {request.PostId} успешно обновлен",
                    null), ct);
                return Result<Guid>.Success(dbPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при обновлении поста {request.PostId}",
                null), ct);
            return Result<Guid>.Failure("Не удалось обновить пост");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при обновлении поста {request?.PostId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении поста: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления поста с ID {id}",
                null), ct);

            var dbPost = await _database.PostRepository.GetByIdAsync(id, ct);
            if (dbPost == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить пост: пост с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пост не найден");
            }

            // Логирование связанных сущностей, которые будут удалены
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Удаление поста {id} с {dbPost.Reactions.Count} реакциями и {dbPost.Comments.Count} комментариями",
                null), ct);

            _database.PostRepository.Delete(dbPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пост {id} успешно удален",
                    null), ct);
                return Result<Guid>.Success(dbPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при удалении поста {id}",
                null), ct);
            return Result<Guid>.Failure("Не удалось удалить пост");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении поста {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при удалении поста: {e.Message}");
        }
    }

    public async Task<Result<List<PostShortDTO>>> GetAllShort(Guid? departmentId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения краткой информации о постах{(departmentId.HasValue ? $" для отдела {departmentId}" : "")}",
                null), ct);

            var posts = departmentId.HasValue
                ? await _database.PostRepository.FindRangeAsync(
                    post => post.DepartmentId == departmentId.Value || !post.DepartmentId.HasValue,
                    ct)
                : await _database.PostRepository.GetAllAsync(ct);

            var dtos = MapToShortDTORange(posts);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} кратких описаний постов{(departmentId.HasValue ? $" для отдела {departmentId}" : "")}",
                null), ct);

            return Result<List<PostShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении краткой информации о постах{(departmentId.HasValue ? $" для отдела {departmentId}" : "")}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<PostShortDTO>>.Failure($"Ошибка при получении постов: {e.Message}");
        }
    }

    public async Task<Result<List<PostDetailedDTO>>> GetAll(Guid? departmentId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения подробной информации о постах{(departmentId.HasValue ? $" для отдела {departmentId}" : "")}",
                null), ct);

            var posts = departmentId.HasValue
                ? await _database.PostRepository.FindRangeAsync(
                    post => post.DepartmentId == departmentId.Value || !post.DepartmentId.HasValue,
                    ct)
                : await _database.PostRepository.GetAllAsync(ct);

            var dtos = MapToDetailedDTORange(posts);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} подробных описаний постов{(departmentId.HasValue ? $" для отдела {departmentId}" : "")}",
                null), ct);

            return Result<List<PostDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении подробной информации о постах{(departmentId.HasValue ? $" для отдела {departmentId}" : "")}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<PostDetailedDTO>>.Failure($"Ошибка при получении постов: {e.Message}");
        }
    }

    public async Task<Result<PostDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения подробной информации о посте с ID {id}",
                null), ct);

            var post = await _database.PostRepository.GetByIdAsync(id, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить пост: пост с ID {id} не найден",
                    null), ct);
                return Result<PostDetailedDTO>.Failure("Пост не найден");
            }

            var dto = MapToDetailedDTO(post);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Подробная информация о посте с ID {id} успешно получена",
                null), ct);

            return Result<PostDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении поста {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<PostDetailedDTO>.Failure($"Ошибка при получении поста: {e.Message}");
        }
    }

    public async Task<Result<PostShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения краткой информации о посте с ID {id}",
                null), ct);

            var post = await _database.PostRepository.GetByIdAsync(id, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось получить пост: пост с ID {id} не найден",
                    null), ct);
                return Result<PostShortDTO>.Failure("Пост не найден");
            }

            var dto = MapToShortDTO(post);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Краткая информация о посте с ID {id} успешно получена",
                null), ct);

            return Result<PostShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении краткой информации о посте {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<PostShortDTO>.Failure($"Ошибка при получении поста: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало блокировки поста с ID {id}",
                null), ct);

            var dbPost = await _database.PostRepository.GetByIdAsync(id, ct);
            if (dbPost == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось заблокировать пост: пост с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пост не найден");
            }

            if (!dbPost.IsActual)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Пост {id} уже заблокирован",
                    null), ct);
                return Result<Guid>.Failure("Пост уже заблокирован");
            }

            dbPost.ModifiedAt = DateTime.UtcNow;
            dbPost.IsActual = false;

            _database.PostRepository.Update(dbPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пост {id} успешно заблокирован",
                    null), ct);
                return Result<Guid>.Success(dbPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при блокировке поста {id}",
                null), ct);
            return Result<Guid>.Failure("Не удалось заблокировать пост");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при блокировке поста {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при блокировке поста: {e.Message}");
        }
    }
}