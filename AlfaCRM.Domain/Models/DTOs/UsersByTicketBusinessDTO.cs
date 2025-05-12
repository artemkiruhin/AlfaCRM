namespace AlfaCRM.Domain.Models.DTOs;

public class UsersByTicketBusinessDTO
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Fullname { get; set; }
    public int TicketsCount { get; set; }
    public int SuggestionsCount { get; set; }
}