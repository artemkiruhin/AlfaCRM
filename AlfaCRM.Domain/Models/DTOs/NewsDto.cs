namespace AlfaCRM.Domain.Models.DTOs;

public class NewsDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public int Watchers { get; set; }
    public Guid PublisherId { get; set; }
    public required string PublisherUsername { get; set; }
    public required string PublisherName { get; set; }
    public required string PublisherSurname { get; set; }
    public required string PublisherPatronymic { get; set; }
    public required string PublisherEmail { get; set; }
    public required string PublisherPhoneNumber { get; set; }
    public List<DepartmentDto> Departments { get; set; } = [];
    public List<NewsCommentDto> Comments { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}