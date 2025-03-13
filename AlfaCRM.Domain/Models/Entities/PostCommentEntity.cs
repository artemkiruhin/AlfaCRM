namespace AlfaCRM.Domain.Models.Entities;

public class PostCommentEntity
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    public Guid PostId { get; set; }
    public Guid SenderId { get; set; }
    
    public virtual PostEntity Post { get; set; } = null!;
    public virtual UserEntity Sender { get; set; } = null!;

    public static PostCommentEntity Create(string content, Guid postId, Guid senderId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Content = content,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            PostId = postId,
            SenderId = senderId
        };
    }
}