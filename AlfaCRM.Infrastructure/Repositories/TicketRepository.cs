using AlfaCRM.Domain.Interfaces.Database.Repositories;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure.Repositories;

public class TicketRepository(AppDbContext context) : BaseRepository<TicketEntity>(context), ITicketRepository
{
    public async Task<IEnumerable<TicketEntity>> GetByDepartmentIdAsync(Guid departmentId, CancellationToken ct)
    {
        var tickets = await DbSet.AsNoTracking().Where(ticket => ticket.DepartmentId == departmentId).ToListAsync(ct);
        return tickets;
    }

    public async Task<IEnumerable<TicketEntity>> GetByCreatorIdAsync(Guid creatorId, CancellationToken ct)
    {
        var tickets = await DbSet.AsNoTracking().Where(ticket => ticket.CreatorId == creatorId).ToListAsync(ct);
        return tickets;
    }
}