using Bsm.Api.Controllers;
using Bsm.Api.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Bsm.Api.Features.Users;

[Authorize(Roles = "Admin")]
public sealed class UsersController(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    IUserBlockingService blockingService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(
        CancellationToken cancellationToken, [FromQuery] string? search = null)
    {
        var query = userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(user =>
                (user.FirstName + " " + user.LastName).Contains(term)
                || (user.Email != null && user.Email.Contains(term)));
        }

        var users = await query
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .Select(user => new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.CreatedOn,
                user.IsBlocked,
            })
            .ToListAsync(cancellationToken);

        var rolesByUser = await dbContext.UserRoles
            .Join(dbContext.Roles, userRole => userRole.RoleId, role => role.Id,
                (userRole, role) => new { userRole.UserId, Role = role.Name })
            .ToListAsync(cancellationToken);
        var roleNamesByUser = rolesByUser
            .GroupBy(userRole => userRole.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(userRole => userRole.Role!).ToArray());

        return Ok(users.Select(user => new UserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedOn,
            roleNamesByUser.GetValueOrDefault(user.Id, Array.Empty<string>()),
            user.IsBlocked)).ToArray());
    }

    [HttpGet("{id}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return user is null ? NotFound() : Ok(await ToResponse(user));
    }

    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (request.IsAdmin && request.IsVerifier)
        {
            return BadRequest(new { message = "Select only one role: Verifier or Admin." });
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return IdentityErrors(result);
        }

        var roles = new List<string>();
        if (request.IsVerifier) roles.Add("Verifier");
        if (request.IsAdmin) roles.Add("Admin");
        if (roles.Count > 0)
        {
            var roleResult = await userManager.AddToRolesAsync(user, roles);
            if (!roleResult.Succeeded)
            {
                return IdentityErrors(roleResult);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, await ToResponse(user));
    }

    [HttpPut("{id}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(
        string id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var isAdmin = request.IsAdmin ?? currentRoles.Contains("Admin");
        var isVerifier = request.IsVerifier ?? currentRoles.Contains("Verifier");
        if (isAdmin && isVerifier)
        {
            return BadRequest(new { message = "Select only one role: Verifier or Admin." });
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        var emailResult = await userManager.SetEmailAsync(user, request.Email);
        if (!emailResult.Succeeded)
        {
            return IdentityErrors(emailResult);
        }

        var usernameResult = await userManager.SetUserNameAsync(user, request.Email);
        if (!usernameResult.Succeeded)
        {
            return IdentityErrors(usernameResult);
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return IdentityErrors(updateResult);
        }

        foreach (var (role, requested) in new[] { ("Verifier", request.IsVerifier), ("Admin", request.IsAdmin) })
        {
            if (requested is not bool enabled) continue;

            var hasRole = await userManager.IsInRoleAsync(user, role);
            if (enabled != hasRole)
            {
                var roleResult = enabled
                    ? await userManager.AddToRoleAsync(user, role)
                    : await userManager.RemoveFromRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    return IdentityErrors(roleResult);
                }
            }
        }

        await transaction.CommitAsync(cancellationToken);
        return Ok(await ToResponse(user));
    }

    /// <summary>Blocks or unblocks a user. Blocking revokes all current sessions.</summary>
    [HttpPut("{id}/blocked")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> SetBlocked(
        string id, SetUserBlockedRequest request, CancellationToken cancellationToken)
    {
        var actingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (actingUserId is null) return Unauthorized();

        var outcome = await blockingService.SetBlockedAsync(
            id, actingUserId, request.IsBlocked, cancellationToken);
        return outcome switch
        {
            UserBlockOutcome.Applied => await GetById(id, cancellationToken),
            UserBlockOutcome.NotFound => NotFound(),
            UserBlockOutcome.SelfBlock => Conflict(new { message = "You cannot block your own account." }),
            UserBlockOutcome.LastActiveAdmin => Conflict(new { message = "The last active Admin cannot be blocked." }),
            _ => Conflict(new { message = "Unable to update this user. Reload and try again." }),
        };
    }

    [HttpPost("{id}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        string id,
        ChangeUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        cancellationToken.ThrowIfCancellationRequested();
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
        return result.Succeeded ? NoContent() : IdentityErrors(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded ? NoContent() : IdentityErrors(result);
    }

    private ActionResult IdentityErrors(IdentityResult result)
    {
        var errors = result.Errors
            .GroupBy(error => error.Code.StartsWith("Duplicate", StringComparison.Ordinal) ? "conflict" : "validation")
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

        return result.Errors.Any(error => error.Code.StartsWith("Duplicate", StringComparison.Ordinal))
            ? Conflict(new { errors })
            : BadRequest(new { errors });
    }

    private async Task<UserResponse> ToResponse(ApplicationUser user) =>
        new(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedOn,
            (await userManager.GetRolesAsync(user)).ToArray(),
            user.IsBlocked);
}