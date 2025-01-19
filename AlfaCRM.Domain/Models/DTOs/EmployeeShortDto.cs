namespace AlfaCRM.Domain.Models.DTOs;

public record EmployeeShortDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public string? Patronymic { get; init; }
    public required string Position { get; init; }
    public required string Department { get; init; }
}