namespace AlfaCRM.Domain.Models.Contracts;

public record ChatCreateRequest(string Name, Guid Creator, bool IsPersonal, List<Guid> MembersIds);