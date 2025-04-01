namespace AlfaCRM.Domain.Models.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime HiredAt { get; set; }
    public DateTime? FiredAt { get; set; }
    public DateTime Birthday { get; set; }
    public bool IsMale { get; set; }
    public bool IsActive { get; set; }
    public bool IsAdmin { get; set; }
    public bool HasPublishedRights { get; set; }
    public bool IsBlocked { get; set; }

    public Guid? DepartmentId { get; set; }
    public virtual DepartmentEntity? Department { get; set; } = null!;

    public virtual ICollection<PostEntity> Posts { get; set; } = [];
    public virtual ICollection<PostCommentEntity> Comments { get; set; } = [];
    public virtual ICollection<PostReactionEntity> Reactions { get; set; } = [];
    public virtual ICollection<TicketEntity> Tickets { get; set; } = [];

    public static UserEntity Create(
        string email,
        string username,
        string passwordHash,
        DateTime hiredAt,
        DateTime birthday,
        bool isMale,
        bool isAdmin,
        bool hasPublishedRights,
        Guid? departmentId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            PasswordHash = passwordHash,
            HiredAt = hiredAt,
            FiredAt = null,
            Birthday = birthday,
            IsMale = isMale,
            IsActive = true,
            IsAdmin = isAdmin,
            HasPublishedRights = hasPublishedRights,
            IsBlocked = false,
            DepartmentId = departmentId,
        };
    }

}