namespace AlfaCRM.Domain.Models.Entities;

public class TaskCaseTypeEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    
    public virtual ICollection<TaskCaseEntity> Tasks { get; set; } = [];
    public virtual ICollection<TaskCaseFastSolutionEntity> FastSolutions { get; set; } = [];
}