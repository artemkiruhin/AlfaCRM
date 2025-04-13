namespace AlfaCRM.Domain.Models.Entities;

public class MessageEntity
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPinned { get; set; }
    public Guid SenderId { get; set; }
    public Guid ChatId { get; set; }
    public Guid? RepliedMessageId { get; set; }
    public virtual UserEntity Sender { get; set; } = null!;
    public virtual ChatEntity Chat { get; set; } = null!;
    public virtual MessageEntity? RepliedMessage { get; set; } = null!;
    public virtual ICollection<MessageEntity> Replies { get; set; } = [];

    public static MessageEntity Create(string content, Guid senderId, Guid? repliedMessageId, Guid chatId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            DeletedAt = null,
            SenderId = senderId,
            ChatId = chatId,
            RepliedMessageId = repliedMessageId,
            IsDeleted = false,
            IsPinned = false
        };
    }
}