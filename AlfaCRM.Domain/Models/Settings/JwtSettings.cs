namespace AlfaCRM.Domain.Models.Settings;

public record JwtSettings(string Audience, string Issuer, string SecurityKey, int ExpireHours);