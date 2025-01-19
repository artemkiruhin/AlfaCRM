namespace AlfaCRM.Domain.Models.DTOs;

public record DepartmentDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public int EmployeesCount { get; init; }
}