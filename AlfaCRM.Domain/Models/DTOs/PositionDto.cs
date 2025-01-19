namespace AlfaCRM.Domain.Models.DTOs;

public class PositionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal Salary { get; set; }
    public bool IsSalaryGross { get; set; }
}