using System.Security.Cryptography;
using System.Text;
using AlfaCRM.Domain.Interfaces.Services.Security;

namespace AlfaCRM.Services.Security;

public class SHA256Hasher : IHashService
{
    public string ComputeHash(string message) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(message)));
}