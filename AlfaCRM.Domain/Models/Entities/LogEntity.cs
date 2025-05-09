namespace AlfaCRM.Domain.Models.Entities;

public class LogEntity
{
    public Guid Id { get; set; }
    public string Message { get; set; }
    public LogType Type { get; set; }
    public Guid? UserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual UserEntity User { get; set; } = null!;

    public static LogEntity Create(LogType logType, string message, Guid? userId)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Type = logType,
            Message = message,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
}