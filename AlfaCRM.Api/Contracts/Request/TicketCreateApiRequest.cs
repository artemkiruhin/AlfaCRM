using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Api.Contracts.Request;

public record TicketCreateApiRequest(string Title, string Text, Guid DepartmentId, TicketType Type);