namespace AlfaCRM.Domain.Models.DTOs;

public record NotWorkingDayBidDto
{
    public Guid Id { get; init; }
    public required EmployeeShortDto Employee { get; init; }
    public required string Type { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime DayFrom { get; init; }
    public DateTime DayTo { get; init; }
    public required FileDto File { get; init; }
}