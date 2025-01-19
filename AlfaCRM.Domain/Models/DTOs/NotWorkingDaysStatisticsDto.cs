namespace AlfaCRM.Domain.Models.DTOs;

public record NotWorkingDaysStatisticsDto
{
    public int TotalDaysOff { get; init; }
    public int VacationDays { get; init; }
    public int SickLeaveDays { get; init; }
    public required IReadOnlyList<NotWorkingDaysByMonthDto> StatisticsByMonth { get; init; } = [];
}