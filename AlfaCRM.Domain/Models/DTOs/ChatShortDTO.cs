namespace AlfaCRM.Domain.Models.DTOs;

public record ChatShortDTO(Guid Id, string Name, DateTime CreatedAt, Guid? AdminId);