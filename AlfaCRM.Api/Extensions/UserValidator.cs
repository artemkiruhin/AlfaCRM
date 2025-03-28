using System.Security.Claims;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Api.Extensions;

public class UserValidator : IUserValidator
{
    private readonly IUserService _userService;

    public UserValidator(IUserService userService)
    {
        _userService = userService;
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
}