using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure;

namespace AlfaCRM.Services.Entity;

public class PostCommentService : IPostCommentService
{
    private readonly UnitOfWork _database;

    public PostCommentService(UnitOfWork database)
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
                Username: entity.Sender.Username,
                Email: entity.Sender.Email,
                DepartmentName: entity.Sender.Department?.Name ?? "Нет отдела"
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
                Username: entity.Sender.Username,
                Email: entity.Sender.Email,
                DepartmentName: entity.Sender.Department?.Name ?? "Нет отдела"
            )
        )).ToList();
    }

    public async Task<Result<Guid>> Create(PostCommentCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var newComment = PostCommentEntity.Create(
                content: request.Content,
                postId: request.PostId,
                senderId: request.SenderId
            );
            
            await _database.PostCommentRepository.CreateAsync(newComment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            return result > 0 
                ? Result<Guid>.Success(newComment.Id) 
                : Result<Guid>.Failure("Failed to create comment");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while creating comment: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var dbComment = await _database.PostCommentRepository.GetByIdAsync(id, ct);
            if (dbComment == null) return Result<Guid>.Failure("Comment not found");

            _database.PostCommentRepository.Delete(dbComment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            
            return result > 0 
                ? Result<Guid>.Success(dbComment.Id) 
                : Result<Guid>.Failure("Failed to delete comment");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while deleting comment: {ex.Message}");
        }
    }

    public async Task<Result<List<PostCommentShortDTO>>> GetAll(Guid postId, CancellationToken ct)
    {
        try
        {
            var comments = await _database.PostCommentRepository.FindRangeAsync(comment => comment.PostId == postId, ct);
            var dtos = MapShortRange(comments);
            
            return Result<List<PostCommentShortDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<PostCommentShortDTO>>.Failure($"Error while retrieving comments: {ex.Message}");
        }
    }

    public async Task<Result<PostCommentShortDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var comment = await _database.PostCommentRepository.GetByIdAsync(id, ct);
            if (comment == null) return Result<PostCommentShortDTO>.Failure("Comment not found");

            var dto = MapShort(comment);
            
            return Result<PostCommentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PostCommentShortDTO>.Failure($"Error while retrieving comment: {ex.Message}");
        }
    }
}