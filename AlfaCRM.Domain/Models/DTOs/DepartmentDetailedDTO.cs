namespace AlfaCRM.Domain.Models.DTOs;

public record DepartmentDetailedDTO(Guid Id, string Name, List<UserShortDTO> Users);