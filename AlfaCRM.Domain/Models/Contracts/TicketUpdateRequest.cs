namespace AlfaCRM.Domain.Models.Contracts;

public record TicketUpdateRequest(Guid Id, string? Title, string? Text, Guid? DepartmentId, string? Feedback, Guid? AssigneeId);