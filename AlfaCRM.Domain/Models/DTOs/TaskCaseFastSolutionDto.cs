namespace AlfaCRM.Domain.Models.DTOs;

public record TaskCaseFastSolutionDto
{
    public Guid Id { get; init; }
    public required string TypeName { get; init; }
    public required string Solution { get; init; }
}