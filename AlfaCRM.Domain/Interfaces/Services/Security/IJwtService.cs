namespace AlfaCRM.Domain.Interfaces.Services.Security;

public interface IJwtService
{
    string GenerateToken(Guid userId);
}