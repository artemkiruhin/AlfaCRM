using System.Security.Claims;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Api.Extensions;

public class UserValidator : IUserValidator
{
    private readonly IUserService _userService;
    private readonly ITicketService _ticketService;

    public UserValidator(IUserService userService, ITicketService ticketService)
    {
        _userService = userService;
        _ticketService = ticketService;
    }
    public async Task<Result<bool>> IsAdminOrPublisher(ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            var idClaimString = user.FindFirst(ClaimTypes.NameIdentifier).Value 
                          ?? throw new NullReferenceException("Claim id not found");
            var id = Guid.Parse(idClaimString);
            
            var userModelResult = await _userService.GetById(id, ct);
            if (!userModelResult.IsSuccess) return Result<bool>.Failure($"Can't find user with id: {id} | {userModelResult.ErrorMessage}");
            
            var userData = userModelResult.Data;
            if (!userData.IsAdmin && !userData.HasPublishedRights) return Result<bool>.Failure("User does not have published rights");
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
    
    public async Task<Result<bool>> IsAdmin(ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            var idClaimString = user.FindFirst(ClaimTypes.NameIdentifier).Value 
                                ?? throw new NullReferenceException("Claim id not found");
            var id = Guid.Parse(idClaimString);
            
            var userModelResult = await _userService.GetById(id, ct);
            if (!userModelResult.IsSuccess) return Result<bool>.Failure($"Can't find user with id: {id} | {userModelResult.ErrorMessage}");
            
            var userData = userModelResult.Data;
            return !userData.IsAdmin ? Result<bool>.Failure("User does not have published rights") : Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
    
    public async Task<Result<Guid>> GetUserId(ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            var idClaimString = user.FindFirst(ClaimTypes.NameIdentifier).Value 
                                ?? throw new NullReferenceException("Claim id not found");
            var id = Guid.Parse(idClaimString);
            
            var userModelResult = await _userService.GetById(id, ct);
            if (!userModelResult.IsSuccess) return Result<Guid>.Failure($"Can't find user with id: {id} | {userModelResult.ErrorMessage}");
            
            return Result<Guid>.Success(userModelResult.Data.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> HasRightsToWorkWithTickets(ClaimsPrincipal user, Guid? ticketAssigneeId, Guid? ticketCreatorId, CancellationToken ct)
    {
        try
        {
            var idClaimString = user.FindFirst(ClaimTypes.NameIdentifier).Value 
                                ?? throw new NullReferenceException("Claim id not found");
            var id = Guid.Parse(idClaimString);
            
            var userModelResult = await _userService.GetById(id, ct);
            if (!userModelResult.IsSuccess) return Result<bool>.Failure($"Can't find user with id: {id} | {userModelResult.ErrorMessage}");
            
            var userData = userModelResult.Data;

            var result = userData.IsAdmin ||
                         (ticketAssigneeId.HasValue && ticketAssigneeId.Value == userData.Id) ||
                         (ticketCreatorId.HasValue && ticketCreatorId.Value == userData.Id);
            return Result<bool>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> IsAdminOrSpecDepartment(ClaimsPrincipal user, CancellationToken ct)
    {
        try
        {
            var idClaimString = user.FindFirst(ClaimTypes.NameIdentifier).Value 
                                ?? throw new NullReferenceException("Claim id not found");
            var id = Guid.Parse(idClaimString);
            
            var userModelResult = await _userService.GetById(id, ct);
            if (!userModelResult.IsSuccess) return Result<bool>.Failure($"Can't find user with id: {id} | {userModelResult.ErrorMessage}");
            
            var userData = userModelResult.Data;
            
            var isAdmin = userData.IsAdmin;
            var hasSpecDepartment = userData.Department is { IsSpecific: true };
            
            return isAdmin || hasSpecDepartment ? Result<bool>.Success(true) : Result<bool>.Failure("User cannot have spec department or admin rights");
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(e.Message);
        }
    }

    public async Task<Result<bool>> IsAdminOrSender(ClaimsPrincipal user, Guid ticketId, CancellationToken ct)
    {
        try
        {
            var idClaimString = user.FindFirst(ClaimTypes.NameIdentifier).Value 
                                ?? throw new NullReferenceException("Claim id not found");
            var id = Guid.Parse(idClaimString);
            
            var userModelResult = await _userService.GetById(id, ct);
            if (!userModelResult.IsSuccess) return Result<bool>.Failure($"Can't find user with id: {id} | {userModelResult.ErrorMessage}");
            
            var userData = userModelResult.Data;
            
            var isAdmin = userData.IsAdmin;
           
            
            var ticket = await _ticketService.GetById(ticketId, ct);
            if (!ticket.IsSuccess) return Result<bool>.Failure($"Can't find ticket with id: {ticketId}");
            
            var isSender = userData.Id == ticket.Data.Creator.Id;
            
            
            return isAdmin || isSender ? Result<bool>.Success(true) : Result<bool>.Failure("User is not sender or hasn't admin rights");
        }
        catch (Exception e)
        {
            return Result<bool>.Failure(e.Message);
        }
    }
}