namespace AlfaCRM.Domain.Models.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime HiredAt { get; set; }
    public DateTime FiredAt { get; set; }
    public DateTime Birthday { get; set; }
    public bool IsMale { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
    public bool HasPublishedRights { get; set; }
    public bool IsBlocked { get; set; }
    
    public Guid DepartmentId { get; set; }
    public virtual DepartmentEntity Department { get; set; } = null!;
    
    public virtual ICollection<PostEntity> Posts { get; set; } = [];
    public virtual ICollection<PostCommentEntity> Comments { get; set; } = [];
    public virtual ICollection<PostReactionEntity> Reactions { get; set; } = [];
}