﻿using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Models.Contracts;

public record TicketCreateRequest(string Title, string Text, Guid DepartmentId, TicketStatus Status, string? Feedback, Guid AssigneeId);