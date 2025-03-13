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

    public static PostReactionEntity Create(Guid postId, Guid senderId, ReactionType type)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            SenderId = senderId,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };
    }
}