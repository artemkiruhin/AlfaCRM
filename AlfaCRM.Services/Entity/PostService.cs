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
                $"Starting post creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.PublisherId), ct);

            // Validate publisher exists
            var publisher = await _database.UserRepository.GetByIdAsync(request.PublisherId, ct);
            if (publisher == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create post: publisher with ID {request.PublisherId} not found",
                    request.PublisherId), ct);
                return Result<Guid>.Failure("Publisher not found");
            }

            // Validate department exists if specified
            if (request.DepartmentId.HasValue)
            {
                var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value, ct);
                if (department == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Failed to create post: department with ID {request.DepartmentId} not found",
                        request.PublisherId), ct);
                    return Result<Guid>.Failure("Department not found");
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
                    $"Post created successfully with ID: {newPost.Id}",
                    request.PublisherId), ct);
                return Result<Guid>.Success(newPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "No changes were made when creating post",
                request.PublisherId), ct);
            return Result<Guid>.Failure("Failed to create post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating post: {e.Message}. StackTrace: {e.StackTrace}",
                request?.PublisherId), ct);
            return Result<Guid>.Failure($"Error while creating post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(PostUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting post update process for ID {request.PostId}. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var dbPost = await _database.PostRepository.GetByIdAsync(request.PostId, ct);
            if (dbPost == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update post: post with ID {request.PostId} not found",
                    null), ct);
                return Result<Guid>.Failure("Post not found");
            }

            // Log changes
            if (!string.IsNullOrEmpty(request.Title) && dbPost.Title != request.Title)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating post {request.PostId} title from '{dbPost.Title}' to '{request.Title}'",
                    null), ct);
                dbPost.Title = request.Title;
            }

            if (!string.IsNullOrEmpty(request.Subtitle) && dbPost.Subtitle != request.Subtitle)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating post {request.PostId} subtitle from '{dbPost.Subtitle}' to '{request.Subtitle}'",
                    null), ct);
                dbPost.Subtitle = request.Subtitle;
            }

            if (!string.IsNullOrEmpty(request.Content) && dbPost.Content != request.Content)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating post {request.PostId} content",
                    null), ct);
                dbPost.Content = request.Content;
            }

            if (request.IsImportant.HasValue && dbPost.IsImportant != request.IsImportant.Value)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating post {request.PostId} IsImportant from {dbPost.IsImportant} to {request.IsImportant.Value}",
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
                            $"Failed to update post: department with ID {request.DepartmentId} not found",
                            null), ct);
                        return Result<Guid>.Failure("Department not found");
                    }
                }

                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating post {request.PostId} DepartmentId from {dbPost.DepartmentId} to {request.DepartmentId}",
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
                    $"Post {request.PostId} updated successfully",
                    null), ct);
                return Result<Guid>.Success(dbPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when updating post {request.PostId}",
                null), ct);
            return Result<Guid>.Failure("Failed to update post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while updating post {request?.PostId}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while updating post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting post deletion process for ID {id}",
                null), ct);

            var dbPost = await _database.PostRepository.GetByIdAsync(id, ct);
            if (dbPost == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete post: post with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("Post not found");
            }

            // Log related entities that will be deleted
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Deleting post {id} with {dbPost.Reactions.Count} reactions and {dbPost.Comments.Count} comments",
                null), ct);

            _database.PostRepository.Delete(dbPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Post {id} deleted successfully",
                    null), ct);
                return Result<Guid>.Success(dbPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when deleting post {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to delete post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting post {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while deleting post: {e.Message}");
        }
    }

    public async Task<Result<List<PostShortDTO>>> GetAllShort(Guid? departmentId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve all short posts{(departmentId.HasValue ? $" for department {departmentId}" : "")}",
                null), ct);

            var posts = departmentId.HasValue
                ? await _database.PostRepository.FindRangeAsync(
                    post => post.DepartmentId == departmentId.Value || !post.DepartmentId.HasValue, 
                    ct)
                : await _database.PostRepository.GetAllAsync(ct);

            var dtos = MapToShortDTORange(posts);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} short posts{(departmentId.HasValue ? $" for department {departmentId}" : "")}",
                null), ct);
            
            return Result<List<PostShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving short posts{(departmentId.HasValue ? $" for department {departmentId}" : "")}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<PostShortDTO>>.Failure($"Error while retrieving posts: {e.Message}");
        }
    }

    public async Task<Result<List<PostDetailedDTO>>> GetAll(Guid? departmentId, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve all detailed posts{(departmentId.HasValue ? $" for department {departmentId}" : "")}",
                null), ct);

            var posts = departmentId.HasValue
                ? await _database.PostRepository.FindRangeAsync(
                    post => post.DepartmentId == departmentId.Value || !post.DepartmentId.HasValue, 
                    ct)
                : await _database.PostRepository.GetAllAsync(ct);

            var dtos = MapToDetailedDTORange(posts);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} detailed posts{(departmentId.HasValue ? $" for department {departmentId}" : "")}",
                null), ct);
            
            return Result<List<PostDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving detailed posts{(departmentId.HasValue ? $" for department {departmentId}" : "")}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<PostDetailedDTO>>.Failure($"Error while retrieving posts: {e.Message}");
        }
    }

    public async Task<Result<PostDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve detailed post with ID {id}",
                null), ct);

            var post = await _database.PostRepository.GetByIdAsync(id, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get post: post with ID {id} not found",
                    null), ct);
                return Result<PostDetailedDTO>.Failure("Post not found");
            }

            var dto = MapToDetailedDTO(post);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved detailed post with ID {id}",
                null), ct);
            
            return Result<PostDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving post {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<PostDetailedDTO>.Failure($"Error while retrieving post: {e.Message}");
        }
    }

    public async Task<Result<PostShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve short post with ID {id}",
                null), ct);

            var post = await _database.PostRepository.GetByIdAsync(id, ct);
            if (post == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to get post: post with ID {id} not found",
                    null), ct);
                return Result<PostShortDTO>.Failure("Post not found");
            }

            var dto = MapToShortDTO(post);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved short post with ID {id}",
                null), ct);
            
            return Result<PostShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving short post {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<PostShortDTO>.Failure($"Error while retrieving post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to block post with ID {id}",
                null), ct);

            var dbPost = await _database.PostRepository.GetByIdAsync(id, ct);
            if (dbPost == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to block post: post with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("Post not found");
            }

            if (!dbPost.IsActual)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Post {id} is already blocked",
                    null), ct);
                return Result<Guid>.Failure("Post is already blocked");
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
                    $"Post {id} blocked successfully",
                    null), ct);
                return Result<Guid>.Success(dbPost.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when blocking post {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to block post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while blocking post {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while blocking post: {e.Message}");
        }
    }
}