using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlfaCRM.Domain.Interfaces.Services.Security;
using AlfaCRM.Domain.Models.Settings;
using Microsoft.IdentityModel.Tokens;

namespace AlfaCRM.Services.Security;

public class JwtService : IJwtService
{
    private readonly byte[] _secret;
    private readonly string _audience;
    private readonly string _issuer;
    private readonly int _expiresInHours;
    
    public JwtService(JwtSettings settings)
    {
        _audience = settings.Audience;
        _issuer = settings.Issuer;
        _secret = Encoding.UTF8.GetBytes(settings.SecurityKey);
        _expiresInHours = settings.ExpireHours;
    }
    
    public string GenerateToken(Guid userId)
    {
        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var key = new SymmetricSecurityKey(_secret);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(_expiresInHours),
            signingCredentials: creds,
            issuer: _issuer,
            audience: _audience
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}