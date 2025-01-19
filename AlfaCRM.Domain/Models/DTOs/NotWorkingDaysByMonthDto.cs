namespace AlfaCRM.Domain.Models.DTOs;

public record NotWorkingDaysByMonthDto
{
    public required string Month { get; init; }
    public int VacationDays { get; init; }
    public int SickLeaveDays { get; init; }
    public int TotalEmployeesOff { get; init; }
}