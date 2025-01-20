namespace AlfaCRM.Domain.Models.Contracts;

public record EmployeeUpdateContract
{
    public string? Name { get; init; }
    public string? Surname { get; init; }
    public string? Patronymic { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime? Birthday { get; init; }
    public bool? HasManagementRights { get; init; }
    public Guid? PositionId { get; init; }
    public Guid? DepartmentId { get; init; }
    public decimal? Salary { get; init; }
    public bool? IsSalaryGross { get; init; }
    public DateTime? HiredAt { get; init; }
    public DateTime? FiredAt { get; init; }
}