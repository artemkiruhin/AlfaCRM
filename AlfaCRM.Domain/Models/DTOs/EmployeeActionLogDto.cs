namespace AlfaCRM.Domain.Models.DTOs;

public record EmployeeActionLogDto
{
    public Guid Id { get; init; }
    public required EmployeeShortDto Employee { get; init; }
    public required string Description { get; init; }
    public DateTime CreatedAt { get; init; }
}