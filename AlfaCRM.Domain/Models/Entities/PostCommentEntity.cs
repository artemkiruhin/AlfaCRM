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
}