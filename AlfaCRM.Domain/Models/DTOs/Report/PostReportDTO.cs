namespace AlfaCRM.Domain.Models.DTOs.Report;

public record PostReportDTO(
    Guid Id,
    string Title,
    string Subtitle,
    string Content,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    bool IsImportant,
    bool IsActual,
    string PublisherUsername,
    string PublisherFullName,
    string DepartmentGuid,
    string DepartmentName,
    int LikesCount,
    int DislikesCount,
    int CommentsCount
);