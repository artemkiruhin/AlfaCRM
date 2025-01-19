namespace AlfaCRM.Domain.Models.DTOs;

public record TaskStatisticsDto
{
    public int TotalTasks { get; init; }
    public int ActiveTasks { get; init; }
    public int CompletedTasks { get; init; }
    public double AverageCompletionTime { get; init; }
    public required IReadOnlyList<TasksByTypeDto> TasksByType { get; init; } = [];
}