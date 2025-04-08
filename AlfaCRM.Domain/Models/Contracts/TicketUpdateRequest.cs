using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Models.Contracts;

public record TicketUpdateRequest(Guid Id, Guid SenderId, string? Title, string? Text, Guid? DepartmentId, string? Feedback, TicketType? Type);