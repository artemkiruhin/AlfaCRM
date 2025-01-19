namespace AlfaCRM.Domain.Models.DTOs;

public record EmployeeDetailedDto : EmployeeBaseDto
{
    public DateTime Birthday { get; init; }
    public int Age { get; init; }
    public DateTime HiredAt { get; init; }
    public DateTime? FiredAt { get; init; }
    public bool IsFired { get; init; }
    public bool HasManagementRights { get; init; }
    public Guid PositionId { get; init; }
    public Guid DepartmentId { get; init; }
    public decimal Salary { get; init; }
    public bool IsSalaryGross { get; init; }
    public int VacationDaysLeft { get; init; }
    public required IReadOnlyList<NotWorkingDayBidDto> NotWorkingDayBids { get; init; } = [];
    public required IReadOnlyList<TaskCaseDto> Tasks { get; init; } = [];
}