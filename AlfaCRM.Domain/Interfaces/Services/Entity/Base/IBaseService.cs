namespace AlfaCRM.Domain.Interfaces.Services.Entity.Base;

public interface IBaseService
{
    Task<bool> DeleteAsync(Guid id);
}