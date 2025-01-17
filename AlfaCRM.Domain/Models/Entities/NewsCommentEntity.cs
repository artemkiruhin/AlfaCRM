namespace AlfaCRM.Domain.Models.Entities;

public class NewsCommentEntity
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    
    public Guid EmployeeId { get; set; }
    public virtual EmployeeEntity Employee { get; set; } = null!;
    
    public Guid? RepliedCommentId { get; set; }
    public virtual NewsCommentEntity? RepliedComment { get; set; } 
    
    public DateTime CreatedAt { get; set; }
}