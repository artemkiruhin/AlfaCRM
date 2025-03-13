namespace AlfaCRM.Domain.Models.Contracts;

public record LoginRequest(
    string Username,
    string PasswordHash
);