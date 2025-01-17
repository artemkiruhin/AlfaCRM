namespace AlfaCRM.Domain.Models.Entities;

public class TaskCaseCommentEntity
{
    public Guid Id { get; set; }
    public Guid PublisherId { get; set; }
    public virtual EmployeeEntity Publisher { get; set; } = null!;
    public Guid TaskCaseId { get; set; }
    public virtual TaskCaseEntity TaskCase { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
