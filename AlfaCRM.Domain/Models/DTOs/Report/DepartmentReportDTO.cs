namespace AlfaCRM.Domain.Models.DTOs.Report;

public record DepartmentReportDTO(Guid Id, string Name, bool IsSpecific, int MembersCount);