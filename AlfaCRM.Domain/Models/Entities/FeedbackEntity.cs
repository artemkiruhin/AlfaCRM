namespace AlfaCRM.Domain.Models.Entities;

public class FeedbackEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Content { get; set; }
    public string? Email { get; set; }
    public FeedbackType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}