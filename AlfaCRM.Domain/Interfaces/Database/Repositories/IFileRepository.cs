using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface IFileRepository : ICrudRepository<FileEntity>
{
    Task<IEnumerable<FileEntity>> GetByType(FileType type, CancellationToken ct);
}