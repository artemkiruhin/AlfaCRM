namespace AlfaCRM.Domain.Models.Entities;

public class PostEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsImportant { get; set; }
    public bool IsActual { get; set; }
    
    public Guid DepartmentId { get; set; }
    public Guid PublisherId { get; set; }
    
    public virtual DepartmentEntity Department { get; set; } = null!;
    public virtual UserEntity Publisher { get; set; } = null!;
    public virtual ICollection<PostReactionEntity> Reactions { get; set; } = [];
    public virtual ICollection<PostCommentEntity> Comments { get; set; } = [];
}