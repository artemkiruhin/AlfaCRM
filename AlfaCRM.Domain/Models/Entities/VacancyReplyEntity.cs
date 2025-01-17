namespace AlfaCRM.Domain.Models.Entities;

public class VacancyReplyEntity
{
    public Guid Id { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public Guid FileId { get; set; }
    public DateTime CreatedAt { get; set; }
}