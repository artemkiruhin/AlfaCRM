using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Extensions;

public interface IStatisticsService
{
   Task<Result<IEnumerable<UsersPerDepartmentByTicketsDTO>>> GetUsersByTicketBusiness(CancellationToken ct);
   Task<Result<bool>> DistributeTicketsByUsers(CancellationToken ct);
   Task<Result<bool>> DistributeTicketToUser(Guid userId, Guid ticketId, CancellationToken ct);
}