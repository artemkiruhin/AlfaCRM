namespace AlfaCRM.Domain.Models.DTOs;

public record TasksByTypeDto
{
    public required string TypeName { get; init; }
    public int Count { get; init; }
    public double PercentageOfTotal { get; init; }
}