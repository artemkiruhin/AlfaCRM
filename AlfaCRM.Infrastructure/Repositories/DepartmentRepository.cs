using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class DepartmentRepository(AppDbContext context) : BaseRepository<DepartmentEntity>(context), IDepartmentRepository
{
    public async Task<DepartmentEntity?> GetDepartmentByName(string name, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(department => department.Name == name, ct);
    }

    public async Task<IEnumerable<DepartmentEntity>> GetDepartmentsBySpecific(bool isSpecific, CancellationToken ct)
    {
        return await DbSet.AsNoTracking().Where(department => department.IsSpecific == isSpecific).ToListAsync(ct);
    }
}