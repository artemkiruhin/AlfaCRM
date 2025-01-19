namespace AlfaCRM.Domain.Models.DTOs;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Content { get; set; }
    public required string Email { get; set; }
    public required string Type { get; set; }
    public DateTime CreatedAt { get; set; }
}