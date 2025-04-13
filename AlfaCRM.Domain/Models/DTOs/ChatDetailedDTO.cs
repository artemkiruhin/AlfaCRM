namespace AlfaCRM.Domain.Models.DTOs;

public record ChatDetailedDTO(Guid Id, string Name, DateTime CreatedAt, UserShortDTO? Admin, MessageDTO Messages, List<UserShortDTO> Members);