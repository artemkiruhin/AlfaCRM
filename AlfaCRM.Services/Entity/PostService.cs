using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories;

namespace AlfaCRM.Services.Entity;

public class PostService : IPostService
{
    private readonly IUnitOfWork _database;

    public PostService(IUnitOfWork database)
    {
        _database = database;
    }
    
    public async Task<Result<Guid>> Create(PostCreateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var newPost = PostEntity.Create(
                title: request.Title,
                subtitle: request.Subtitle,
                content: request.Content,
                isImportant: request.IsImportant,
                departmentId: request.DepartmentId,
                publisherId: request.PublisherId
            );
            
            await _database.PostRepository.CreateAsync(newPost);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(newPost.Id) 
                : Result<Guid>.Failure("Failed to create post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while creating post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(PostUpdateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var dbPost = await _database.PostRepository.GetByIdAsync(request.PostId);
            if (dbPost == null) return Result<Guid>.Failure("Post not found");
            
            if (!string.IsNullOrEmpty(request.Title)) dbPost.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Subtitle)) dbPost.Subtitle = request.Subtitle;
            if (!string.IsNullOrEmpty(request.Content)) dbPost.Content = request.Content;
            if (request.IsImportant.HasValue) dbPost.IsImportant = request.IsImportant.Value;
            if (request.DepartmentId.HasValue) dbPost.DepartmentId = request.DepartmentId.Value;
            
            dbPost.ModifiedAt = DateTime.UtcNow;
            
            _database.PostRepository.Update(dbPost);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(dbPost.Id) 
                : Result<Guid>.Failure("Failed to update post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while updating post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var dbPost = await _database.PostRepository.GetByIdAsync(id);
            if (dbPost == null) return Result<Guid>.Failure("Post not found");
            
            _database.PostRepository.Delete(dbPost);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(dbPost.Id) 
                : Result<Guid>.Failure("Failed to delete post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while deleting post: {e.Message}");
        }
    }

    public async Task<Result<List<PostShortDTO>>> GetAllShort(Guid? departmentId)
    {
        try
        {
            var posts = departmentId.HasValue
                ? await _database.PostRepository.GetPostForDepartment(departmentId.Value)
                : await _database.PostRepository.GetAllAsync();
        
            var dtos = posts.Select(post => new PostShortDTO(
                Id: post.Id,
                Title: post.Title
            )).ToList();
        
            return Result<List<PostShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<PostShortDTO>>.Failure($"Error while retrieving posts: {e.Message}");
        }
    }

    public async Task<Result<List<PostDetailedDTO>>> GetAll(Guid? departmentId)
    {
        try
        {
            var posts = departmentId.HasValue
                ? await _database.PostRepository.GetPostForDepartment(departmentId.Value)
                : await _database.PostRepository.GetAllAsync();

            var dtos = posts.Select(post => new PostDetailedDTO(
                Id: post.Id,
                Title: post.Title,
                Subtitle: post.Subtitle,
                Content: post.Content,
                CreatedAt: post.CreatedAt,
                ModifiedAt: post.ModifiedAt,
                IsImportant: post.IsImportant,
                IsActual: post.IsActual,
                Publisher: new UserShortDTO(
                    Id: post.PublisherId,
                    Username: post.Publisher.Username,
                    Email: post.Publisher.Email,
                    DepartmentName: post.Publisher.Department.Name
                ),
                Department: post.DepartmentId.HasValue
                    ? new DepartmentShortDTO(
                        Id: post.DepartmentId.Value,
                        Name: post.Department.Name
                    )
                    : null,
                Reactions: post.Reactions.Select(reaction => new PostReactionShortDTO(
                    Id: reaction.Id,
                    Sender: new UserShortDTO(
                        Id: reaction.SenderId,
                        Username: reaction.Sender.Username,
                        Email: reaction.Sender.Email,
                        DepartmentName: reaction.Sender.Department.Name
                    ),
                    CreatedAt: reaction.CreatedAt,
                    Type: nameof(reaction.Type)
                )).ToList(),
                Comments: post.Comments.Select(comment => new PostCommentShortDTO(
                    Id: comment.Id,
                    Content: comment.Content,
                    IsDeleted: comment.IsDeleted,
                    CreatedAt: comment.CreatedAt,
                    Sender: new UserShortDTO(
                        Id: comment.SenderId,
                        Username: comment.Sender.Username,
                        Email: comment.Sender.Email,
                        DepartmentName: comment.Sender.Department.Name
                    )
                )).ToList()
            )).ToList();

            return Result<List<PostDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<PostDetailedDTO>>.Failure($"Error while retrieving posts: {e.Message}");
        }
    }

    public async Task<Result<PostDetailedDTO>> GetById(Guid id)
    {
        try
        {
            var post = await _database.PostRepository.GetByIdAsync(id);
            if (post == null) return Result<PostDetailedDTO>.Failure("Post not found");

            var dto = new PostDetailedDTO(
                Id: post.Id,
                Title: post.Title,
                Subtitle: post.Subtitle,
                Content: post.Content,
                CreatedAt: post.CreatedAt,
                ModifiedAt: post.ModifiedAt,
                IsImportant: post.IsImportant,
                IsActual: post.IsActual,
                Publisher: new UserShortDTO(
                    Id: post.PublisherId,
                    Username: post.Publisher.Username,
                    Email: post.Publisher.Email,
                    DepartmentName: post.Publisher.Department.Name
                ),
                Department: post.DepartmentId.HasValue
                    ? new DepartmentShortDTO(
                        Id: post.DepartmentId.Value,
                        Name: post.Department.Name
                    )
                    : null,
                Reactions: post.Reactions.Select(reaction => new PostReactionShortDTO(
                    Id: reaction.Id,
                    Sender: new UserShortDTO(
                        Id: reaction.SenderId,
                        Username: reaction.Sender.Username,
                        Email: reaction.Sender.Email,
                        DepartmentName: reaction.Sender.Department.Name
                    ),
                    CreatedAt: reaction.CreatedAt,
                    Type: nameof(reaction.Type)
                )).ToList(),
                Comments: post.Comments.Select(comment => new PostCommentShortDTO(
                    Id: comment.Id,
                    Content: comment.Content,
                    IsDeleted: comment.IsDeleted,
                    CreatedAt: comment.CreatedAt,
                    Sender: new UserShortDTO(
                        Id: comment.SenderId,
                        Username: comment.Sender.Username,
                        Email: comment.Sender.Email,
                        DepartmentName: comment.Sender.Department.Name
                    )
                )).ToList()
            );

            return Result<PostDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<PostDetailedDTO>.Failure($"Error while retrieving post: {e.Message}");
        }
    }

    public async Task<Result<PostShortDTO>> GetByIdShort(Guid id)
    {
        try
        {
            var post = await _database.PostRepository.GetByIdAsync(id);
            if (post == null) return Result<PostShortDTO>.Failure("Post not found");
        
            var dto = new PostShortDTO(
                Id: post.Id,
                Title: post.Title
            );
        
            return Result<PostShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<PostShortDTO>.Failure($"Error while retrieving post: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var dbPost = await _database.PostRepository.GetByIdAsync(id);
            if (dbPost == null) return Result<Guid>.Failure("Post not found");
            
            dbPost.ModifiedAt = DateTime.UtcNow;
            dbPost.IsActual = false;
            
            _database.PostRepository.Update(dbPost);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(dbPost.Id) 
                : Result<Guid>.Failure("Failed to block post");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while blocking post: {e.Message}");
        }
    }
}