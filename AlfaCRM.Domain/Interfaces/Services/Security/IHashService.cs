namespace AlfaCRM.Domain.Interfaces.Services.Security;

public interface IHashService
{
    string ComputeHash(string message);
}