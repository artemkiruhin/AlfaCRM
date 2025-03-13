namespace AlfaCRM.Domain.Models.Contracts;

public record ResetPasswordRequest(
    Guid UserId,
    string NewPasswordHash
);