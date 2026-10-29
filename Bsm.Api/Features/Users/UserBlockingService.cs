using System.Data;
using Bsm.Api.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bsm.Api.Features.Users;

public enum UserBlockOutcome { Applied, NotFound, SelfBlock, LastActiveAdmin, Conflict }

public interface IUserBlockingService
{
    Task<UserBlockOutcome> SetBlockedAsync(
        string userId, string actingUserId, bool isBlocked, CancellationToken cancellationToken);
}

public sealed class UserBlockingService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    ILogger<UserBlockingService> logger) : IUserBlockingService
{
    public async Task<UserBlockOutcome> SetBlockedAsync(
        string userId, string actingUserId, bool isBlocked, CancellationToken cancellationToken)
    {
        if (isBlocked && userId == actingUserId)
        {
            return UserBlockOutcome.SelfBlock;
        }

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var user = await dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        if (user is null) return UserBlockOutcome.NotFound;
        if (user.IsBlocked == isBlocked) return UserBlockOutcome.Applied;

        if (isBlocked)
        {
            var adminUserIds = dbContext.UserRoles
                .Join(dbContext.Roles.Where(role => role.NormalizedName == "ADMIN"),
                    userRole => userRole.RoleId, role => role.Id, (userRole, role) => userRole.UserId);
            if (await adminUserIds.ContainsAsync(userId, cancellationToken)
                && !await dbContext.Users.AnyAsync(
                    other => other.Id != userId && !other.IsBlocked && adminUserIds.Contains(other.Id),
                    cancellationToken))
            {
                return UserBlockOutcome.LastActiveAdmin;
            }
        }

        user.IsBlocked = isBlocked;
        user.TokenVersion = checked(user.TokenVersion + 1);
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            logger.LogWarning("Unable to change blocked status for user {UserId}: {Errors}",
                userId, string.Join("; ", result.Errors.Select(error => error.Code)));
            return UserBlockOutcome.Conflict;
        }

        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Admin {AdminId} set user {UserId} blocked status to {IsBlocked}",
            actingUserId, userId, isBlocked);
        return UserBlockOutcome.Applied;
    }
}
