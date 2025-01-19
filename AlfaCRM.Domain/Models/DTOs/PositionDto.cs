namespace AlfaCRM.Domain.Models.DTOs;

public record PositionDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public decimal Salary { get; init; }
    public bool IsSalaryGross { get; init; }
    public int EmployeesCount { get; init; }
}