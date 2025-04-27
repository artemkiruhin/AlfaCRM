namespace AlfaCRM.Domain.Models.DTOs;

public record UserProfileDTO(
    Guid Id,
    string Fullname,
    string Email,
    string Username,
    string Birthday,
    string HiredAt,
    string FiredAt,
    bool IsMale,
    bool IsAdmin,
    bool HasPublishedRights,
    string DepartmentName,
    int PostsAmount,
    int CommentsAmount,
    int MessagesAmount
);