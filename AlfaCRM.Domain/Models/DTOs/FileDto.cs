namespace AlfaCRM.Domain.Models.DTOs;

public record FileDto
{
    public Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string FilePath { get; init; }
    public DateTime UploadedAt { get; init; }
}