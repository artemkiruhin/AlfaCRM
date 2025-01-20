namespace AlfaCRM.Domain.Interfaces.Services.Security;

public interface IJwtService
{
    string CreateToken(Guid userId);
    bool ValidateToken(string token);
    Guid GetUserIdFromToken(string token);
}