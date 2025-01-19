namespace AlfaCRM.Domain.Models.DTOs;

public record VacancyDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public decimal Salary { get; init; }
    public bool IsSalaryGross { get; init; }
    public required string ShiftType { get; init; }
    public TimeSpan WorkTimeFrom { get; init; }
    public TimeSpan WorkTimeTo { get; init; }
    public bool AllowsRemoteWork { get; init; }
    public int ExperienceFrom { get; init; }
    public int ExperienceTo { get; init; }
    public int Watchers { get; init; }
    public int ApplicationsCount { get; init; }
    public required EmployeeShortDto Publisher { get; init; }
    public required string Type { get; init; }
    public DateTime CreatedAt { get; init; }
    public TimeSpan ActiveDuration { get; init; }
    public required IReadOnlyList<VacancyReplyDto> Replies { get; init; } = [];
}