namespace AlfaCRM.Domain.Models.Entities;

public class NewsEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public int Watchers { get; set; }
    
    
    public Guid PublisherId { get; set; }
    public virtual EmployeeEntity Publisher { get; set; } = null!;

    public virtual ICollection<DepartmentEntity> Departments { get; set; } = [];
    public virtual ICollection<NewsCommentEntity> Comments { get; set; } = [];
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}