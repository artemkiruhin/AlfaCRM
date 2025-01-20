namespace AlfaCRM.Domain.Models.Contracts;

public record EmployeeCreateContract
{
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? Patronymic { get; set; }
    public string? Email { get; set; }
    public required string PhoneNumber { get; set; }
    public bool HasManagementRights { get; set; }
    public DateTime Birthday { get; set; }
    public DateTime HiredAt { get; set; }
    public Guid PositionId { get; set; }
    public Guid DepartmentId { get; set; }
};