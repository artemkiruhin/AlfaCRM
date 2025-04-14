namespace AlfaCRM.Domain.Models.DTOs;

public record ChatDetailedDTO(Guid Id, string Name, DateTime CreatedAt, UserShortDTO? Admin, List<MessageDTO> Messages, List<UserShortDTO> Members);