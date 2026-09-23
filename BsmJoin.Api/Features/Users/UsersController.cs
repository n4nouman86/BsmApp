using BsmJoin.Api.Controllers;
using BsmJoin.Api.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BsmJoin.Api.Features.Users;

[Authorize(Roles = "Admin")]
public sealed class UsersController(UserManager<ApplicationUser> userManager) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .AsNoTracking()
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
            })
            .ToListAsync(cancellationToken);

        return Ok(users.Select(user => new UserResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedOn)).ToArray());
    }

    [HttpGet("{id}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
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

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResponse(user));
    }

    [HttpPut("{id}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(string id, UpdateUserRequest request)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
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
        return updateResult.Succeeded
            ? Ok(ToResponse(user))
            : IdentityErrors(updateResult);
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

    private static UserResponse ToResponse(ApplicationUser user) =>
        new(user.Id, user.UserName, user.Email, user.FirstName, user.LastName, user.CreatedOn);
}