using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Api.Contracts.Request;

public record TicketChangeStatusApiRequest(Guid Id, TicketStatus Status, string? Feedback);