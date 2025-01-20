namespace AlfaCRM.Domain.Models.Contracts;

public record EmployeeQueryParameters
{
    public required EmployeeFilterContract Filter { get; init; }
    public required EmployeeSortingParameters SortParameter { get; init; }
}