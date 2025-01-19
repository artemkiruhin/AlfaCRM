namespace AlfaCRM.Domain.Models.DTOs;

public record VacancyReplyDto
{
    public Guid Id { get; init; }
    public required string FullName { get; init; }
    public required string PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? Description { get; init; }
    public required FileDto File { get; init; }
    public DateTime CreatedAt { get; init; }
}