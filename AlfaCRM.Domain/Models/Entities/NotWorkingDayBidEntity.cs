namespace AlfaCRM.Domain.Models.Entities;

public class NotWorkingDayBidEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public virtual EmployeeEntity Employee { get; set; } = null!;
    public BidTypeEntity Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DayFrom { get; set; }
    public DateTime DayTo { get; set; }
    public Guid FileId { get; set; }
    public virtual FileEntity File { get; set; } = null!;
}