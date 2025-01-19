namespace AlfaCRM.Domain.Models.DTOs;

public record LogDto
{
    public Guid Id { get; init; }
    public required string Message { get; init; }
    public required string Type { get; init; }
    public DateTime CreatedAt { get; init; }
}