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
                Username: entity.Publisher.Username,
                Email: entity.Publisher.Email,
                DepartmentName: entity.Publisher.Department?.Name ?? "Нет отдела"
            ),
            Department: entity.DepartmentId.HasValue
                ? new DepartmentShortDTO(
                    Id: entity.DepartmentId.Value,
                    Name: entity.Department.Name
                )
                : null,
            Reactions: entity.Reactions.Select(reaction => new PostReactionShortDTO(
                Id: reaction.Id,
                Sender: new UserShortDTO(
                    Id: reaction.SenderId,
                    Username: reaction.Sender.Username,
                    Email: reaction.Sender.Email,
                    DepartmentName: reaction.Sender.Department?.Name ?? "Нет отдела"
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
                    Username: comment.Sender.Username,
                    Email: comment.Sender.Email,
                    DepartmentName: comment.Sender.Department?.Name ?? "Нет отдела"
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
                Username: entity.Publisher.Username,
                Email: entity.Publisher.Email,
                DepartmentName: entity.Publisher.Department?.Name ?? "Нет отдела"
            ),
            Department: entity.DepartmentId.HasValue
                ? new DepartmentShortDTO(
                    Id: entity.DepartmentId.Value,
                    Name: entity.Department.Name
                )
                : null,
            Reactions: entity.Reactions.Select(reaction => new PostReactionShortDTO(
                Id: reaction.Id,
                Sender: new UserShortDTO(
                    Id: reaction.SenderId,
                    Username: reaction.Sender.Username,
                    Email: reaction.Sender.Email,
                    DepartmentName: reaction.Sender.Department?.Name ?? "Нет отдела"
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
                    Username: comment.Sender.Username,
                    Email: comment.Sender.Email,
                    DepartmentName: comment.Sender.Department?.Name ?? "Нет отдела"
                )
            )).ToList()
        )).ToList();
    }

    public async Task<Result<Guid>> Create(PostCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var newPost = PostEntity.Create(
                title: request.Title,
                subtitle: request.Subtitle,
                content: request.Content,
                isImportant: request.IsImportant,
                departmentId: request.DepartmentId.HasValue ? request.DepartmentId.Value : null,
                publisherId: request.PublisherId
            );

            await _database.PostRepository.CreateAsync(newPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(newPost.Id)
                : Result<Guid>.Failure("Failed to create post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while creating post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(PostUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var dbPost = await _database.PostRepository.GetByIdAsync(request.PostId, ct);
            if (dbPost == null) return Result<Guid>.Failure("Post not found");

            if (!string.IsNullOrEmpty(request.Title)) dbPost.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Subtitle)) dbPost.Subtitle = request.Subtitle;
            if (!string.IsNullOrEmpty(request.Content)) dbPost.Content = request.Content;
            if (request.IsImportant.HasValue) dbPost.IsImportant = request.IsImportant.Value;
            if (request.DepartmentId.HasValue || request.EditDepartment) dbPost.DepartmentId = request.DepartmentId;

            dbPost.ModifiedAt = DateTime.UtcNow;

            _database.PostRepository.Update(dbPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(dbPost.Id)
                : Result<Guid>.Failure("Failed to update post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while updating post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var dbPost = await _database.PostRepository.GetByIdAsync(id, ct);
            if (dbPost == null) return Result<Guid>.Failure("Post not found");

            _database.PostRepository.Delete(dbPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(dbPost.Id)
                : Result<Guid>.Failure("Failed to delete post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while deleting post: {e.Message}");
        }
    }

    public async Task<Result<List<PostShortDTO>>> GetAllShort(Guid? departmentId, CancellationToken ct)
    {
        try
        {
            var posts = departmentId.HasValue
                ? await _database.PostRepository.FindRangeAsync(post => post.DepartmentId == departmentId.Value || !post.DepartmentId.HasValue, ct)
                : await _database.PostRepository.GetAllAsync(ct);

            var dtos = MapToShortDTORange(posts);
            return Result<List<PostShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<PostShortDTO>>.Failure($"Error while retrieving posts: {e.Message}");
        }
    }

    public async Task<Result<List<PostDetailedDTO>>> GetAll(Guid? departmentId, CancellationToken ct)
    {
        try
        {
            var posts = departmentId.HasValue
                ? await _database.PostRepository.FindRangeAsync(post => post.DepartmentId == departmentId.Value || !post.DepartmentId.HasValue, ct)
                : await _database.PostRepository.GetAllAsync(ct);

            var dtos = MapToDetailedDTORange(posts);
            return Result<List<PostDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<PostDetailedDTO>>.Failure($"Error while retrieving posts: {e.Message}");
        }
    }

    public async Task<Result<PostDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var post = await _database.PostRepository.GetByIdAsync(id, ct);
            if (post == null) return Result<PostDetailedDTO>.Failure("Post not found");

            var dto = MapToDetailedDTO(post);
            return Result<PostDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<PostDetailedDTO>.Failure($"Error while retrieving post: {e.Message}");
        }
    }

    public async Task<Result<PostShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            var post = await _database.PostRepository.GetByIdAsync(id, ct);
            if (post == null) return Result<PostShortDTO>.Failure("Post not found");

            var dto = MapToShortDTO(post);
            return Result<PostShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<PostShortDTO>.Failure($"Error while retrieving post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var dbPost = await _database.PostRepository.GetByIdAsync(id, ct);
            if (dbPost == null) return Result<Guid>.Failure("Post not found");

            dbPost.ModifiedAt = DateTime.UtcNow;
            dbPost.IsActual = false;

            _database.PostRepository.Update(dbPost, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(dbPost.Id)
                : Result<Guid>.Failure("Failed to block post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while blocking post: {e.Message}");
        }
    }
}