namespace AlfaCRM.Domain.Models.DTOs;

public record DepartmentStatisticsDto
{
    public Guid DepartmentId { get; init; }
    public required string DepartmentName { get; init; }
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int VacationEmployees { get; init; }
    public int SickLeaveEmployees { get; init; }
    public decimal AverageSalary { get; init; }
}