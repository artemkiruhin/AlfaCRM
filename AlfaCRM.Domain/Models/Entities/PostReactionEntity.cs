namespace AlfaCRM.Domain.Models.Entities;

public class PostReactionEntity
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid SenderId { get; set; }
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public virtual PostEntity Post { get; set; } = null!;
    public virtual UserEntity Sender { get; set; } = null!;
}