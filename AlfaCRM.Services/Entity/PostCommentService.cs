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

    public async Task<bool> Create(PostCommentCreateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var newComment = PostCommentEntity.Create(
                content: request.Content,
                postId: request.PostId,
                senderId: request.SenderId
            );
            
            
            await _database.PostCommentRepository.CreateAsync(newComment);
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
            var dbComment = await _database.PostCommentRepository.GetByIdAsync(id);
            if (dbComment == null) throw new KeyNotFoundException();

            
            _database.PostCommentRepository.Delete(dbComment);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0;
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task<List<PostCommentShortDTO>> GetAll(Guid postId)
    {
        var comments = await _database.PostCommentRepository.FindRangeAsync(comment => comment.PostId == postId);
        var dtos = comments.Select(comment => new PostCommentShortDTO(
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
        )).ToList();
        
        return dtos;
    }

    public async Task<PostCommentShortDTO> GetById(Guid id)
    {
        var comment = await _database.PostCommentRepository.GetByIdAsync(id);
        if (comment == null) throw new KeyNotFoundException();

        var dto = new PostCommentShortDTO(
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
        );
        
        return dto;
    }
}