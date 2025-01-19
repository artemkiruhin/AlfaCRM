namespace AlfaCRM.Domain.Models.DTOs;

public record EmployeeBaseDto
{
    public Guid Id { get; init; }
    public required string Username { get; init; }
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public string? Patronymic { get; init; }
    public string? Email { get; init; }
    public required string PhoneNumber { get; init; }
    public required string Position { get; init; }
    public required string Department { get; init; }
}