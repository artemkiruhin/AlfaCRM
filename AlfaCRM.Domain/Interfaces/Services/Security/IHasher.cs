namespace AlfaCRM.Domain.Interfaces.Services.Security;

public interface IHasher
{
    string ComputeHash(string message);
}