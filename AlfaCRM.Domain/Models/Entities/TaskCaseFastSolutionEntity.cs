namespace AlfaCRM.Domain.Models.Entities;

public class TaskCaseFastSolutionEntity
{
    public Guid Id { get; set; }
    public Guid TypeId { get; set; }
    public virtual TaskCaseTypeEntity Type { get; set; } = null!;
}