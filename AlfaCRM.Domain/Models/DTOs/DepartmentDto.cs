namespace AlfaCRM.Domain.Models.DTOs;

public record DepartmentDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}