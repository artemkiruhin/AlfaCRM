namespace AlfaCRM.Domain.Models.Contracts;

public record EmployeeFilterContract
{
    public string? SearchTerm { get; init; }
    public DateTime? HiredFrom { get; init; }
    public DateTime? HiredTo { get; init; }
    public DateTime? BirthdayFrom { get; init; }
    public DateTime? BirthdayTo { get; init; }
    public bool? IsFired { get; init; }
    public bool? HasManagementRights { get; init; }
    public Guid? PositionId { get; init; }
    public Guid? DepartmentId { get; init; }
    public decimal? MinSalary { get; init; }
    public decimal? MaxSalary { get; init; }
    public bool? IsSalaryGross { get; init; }
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
}