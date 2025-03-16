namespace AlfaCRM.Api.Contracts.Request;

public record ResetPasswordBodyRequest(Guid UserId, string NewPassword, bool MustValidate, string? OldPassword);