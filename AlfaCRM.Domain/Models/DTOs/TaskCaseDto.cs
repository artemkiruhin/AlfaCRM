namespace AlfaCRM.Domain.Models.DTOs;

public record TaskCaseDto
{
    public Guid Id { get; init; }
    public required EmployeeShortDto Employee { get; init; }
    public required EmployeeShortDto Helper { get; init; }
    public required string Status { get; init; }
    public required string Type { get; init; }
    public required string Description { get; init; }
    public TimeSpan TimeInCurrentStatus { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public bool IsClosed { get; init; }
    public required IReadOnlyList<TaskCaseCommentDto> Comments { get; init; } = [];
}