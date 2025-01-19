namespace AlfaCRM.Domain.Models.DTOs;

public record TaskCaseTypeDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public int ActiveTasksCount { get; init; }
    public required IReadOnlyList<TaskCaseFastSolutionDto> FastSolutions { get; init; } = [];
}