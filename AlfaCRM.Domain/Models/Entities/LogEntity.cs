namespace AlfaCRM.Domain.Models.Entities;

public class LogEntity
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
    public LogTypeEntity Type { get; set; }
    public DateTime CreatedAt { get; set; }
}