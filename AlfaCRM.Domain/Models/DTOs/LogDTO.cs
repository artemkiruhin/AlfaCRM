namespace AlfaCRM.Domain.Models.DTOs;

public record LogDTO
(
    Guid Id,
    string Message,
    string Type,
    string UserIdString,
    string Username,
    DateTime CreatedAt 
);