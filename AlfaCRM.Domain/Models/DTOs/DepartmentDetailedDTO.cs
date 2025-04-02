namespace AlfaCRM.Domain.Models.DTOs;

public record DepartmentDetailedDTO(Guid Id, string Name, bool IsSpecific, List<UserShortDTO> Users);