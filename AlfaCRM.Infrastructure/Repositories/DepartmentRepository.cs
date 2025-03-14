using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class DepartmentRepository(AppDbContext context) : BaseRepository<DepartmentEntity>(context), IDepartmentRepository
{
    public async Task<DepartmentEntity?> GetDepartmentByName(string name)
    {
        return await DbSet.AsNoTracking().FirstOrDefaultAsync(department => department.Name == name);
    }
}