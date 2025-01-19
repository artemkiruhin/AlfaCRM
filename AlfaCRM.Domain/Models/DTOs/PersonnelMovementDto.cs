namespace AlfaCRM.Domain.Models.DTOs;

public record PersonnelMovementDto
{
    public Guid Id { get; init; }
    public required EmployeeShortDto Employee { get; init; }
    public required EmployeeShortDto HR { get; init; }
    public string? PositionFrom { get; init; }
    public string? PositionTo { get; init; }
    public DateTime CreatedAt { get; init; }
}