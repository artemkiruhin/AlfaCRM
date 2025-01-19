namespace AlfaCRM.Domain.Models.DTOs;

public class LogDto
{
    public Guid Id { get; set; }
    public required string Message { get; set; }
    public required string Type { get; set; }
    public DateTime CreatedAt { get; set; }
}