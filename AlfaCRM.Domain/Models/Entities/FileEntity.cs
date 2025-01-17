namespace AlfaCRM.Domain.Models.Entities;

public class FileEntity
{
    public Guid Id { get; set; }
    public required string FileName { get; set; } 
    public required string FilePath { get; set; } 
    public DateTime UploadedAt { get; set; } 
    
    public Guid? VacancyId { get; set; }
    
    public Guid? NotWorkingDayBidId { get; set; }
}