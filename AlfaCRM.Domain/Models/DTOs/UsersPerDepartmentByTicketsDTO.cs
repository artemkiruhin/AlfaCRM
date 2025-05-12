namespace AlfaCRM.Domain.Models.DTOs;

public class UsersPerDepartmentByTicketsDTO
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public List<UsersByTicketBusinessDTO> Users { get; set; }
}