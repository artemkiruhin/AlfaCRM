namespace AlfaCRM.Api.Contracts.Request;

public record TicketUpdateApiRequest(Guid Id, string? Title, string? Text, Guid? DepartmentId, string? Feedback);