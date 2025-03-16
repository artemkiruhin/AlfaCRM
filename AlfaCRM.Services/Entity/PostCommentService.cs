﻿using AlfaCRM.Domain.Interfaces.Services.Entity;
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

    public async Task<Result<Guid>> Create(PostCommentCreateRequest request)
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
            
            return result > 0 
                ? Result<Guid>.Success(newComment.Id) 
                : Result<Guid>.Failure("Failed to create comment");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while creating comment: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var dbComment = await _database.PostCommentRepository.GetByIdAsync(id);
            if (dbComment == null) return Result<Guid>.Failure("Comment not found");

            _database.PostCommentRepository.Delete(dbComment);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(dbComment.Id) 
                : Result<Guid>.Failure("Failed to delete comment");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while deleting comment: {ex.Message}");
        }
    }

    public async Task<Result<List<PostCommentShortDTO>>> GetAll(Guid postId)
    {
        try
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
            
            return Result<List<PostCommentShortDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<PostCommentShortDTO>>.Failure($"Error while retrieving comments: {ex.Message}");
        }
    }

    public async Task<Result<PostCommentShortDTO>> GetById(Guid id)
    {
        try
        {
            var comment = await _database.PostCommentRepository.GetByIdAsync(id);
            if (comment == null) return Result<PostCommentShortDTO>.Failure("Comment not found");

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
            
            return Result<PostCommentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<PostCommentShortDTO>.Failure($"Error while retrieving comment: {ex.Message}");
        }
    }
}