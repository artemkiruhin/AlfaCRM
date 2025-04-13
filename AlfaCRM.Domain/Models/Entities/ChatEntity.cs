namespace AlfaCRM.Domain.Models.Entities;

public class ChatEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? AdminId { get; set; }
    public virtual UserEntity? Admin { get; set; }
    public virtual ICollection<MessageEntity> Messages { get; set; } = [];
    public virtual ICollection<UserEntity> Members { get; set; } = [];

    public static ChatEntity Create(string name, Guid? adminId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            AdminId = adminId,
            Name = name
        };
    }
}