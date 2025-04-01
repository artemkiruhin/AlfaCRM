namespace AlfaCRM.Domain.Models.Entities;

public class TicketEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Text { get; set; }
    public string? Feedback { get; set; }
    public Guid CreatorId { get; set; }
    public Guid DepartmentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public TicketStatus Status { get; set; }
    public Guid AssigneeId { get; set; }
    public DateTime? ClosedAt { get; set; }

    public virtual UserEntity Creator { get; set; } = null!;
    public virtual DepartmentEntity Department { get; set; } = null!;
    public virtual UserEntity Assignee { get; set; } = null!;

    public static TicketEntity Create(string title, string text, Guid departmentId, TicketStatus status, string? feedback, Guid assigneeId, Guid creatorId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            Text = text,
            DepartmentId = departmentId,
            CreatedAt = DateTime.UtcNow,
            Status = status,
            AssigneeId = assigneeId,
            Feedback = status switch
            {
                TicketStatus.Created => null,
                TicketStatus.InWork => null,
                TicketStatus.Completed => feedback ?? throw new NullReferenceException("Feedback cannot be null."),
                TicketStatus.Rejected => feedback ?? throw new NullReferenceException("Feedback cannot be null."),
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            },
            CreatorId = creatorId
        };
    }
}