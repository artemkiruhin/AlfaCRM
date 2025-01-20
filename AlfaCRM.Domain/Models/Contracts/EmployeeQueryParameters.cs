namespace AlfaCRM.Domain.Models.Contracts;

public record EmployeeSortingParameters
{
    public required string PropertyName { get; init; }
    public bool IsDescending { get; init; }
}