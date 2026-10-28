using System.ComponentModel.DataAnnotations;

namespace Bsm.Api.Features.Users;

/// <summary>Payload used to create an application user.</summary>
public sealed record CreateUserRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress, MaxLength(256)]
    public required string Email { get; init; }

    [Required, MinLength(8), MaxLength(100)]
    public required string Password { get; init; }
}

/// <summary>Payload used to update an application user's profile.</summary>
public sealed record UpdateUserRequest
{
    [Required, MaxLength(100)]
    public required string FirstName { get; init; }

    [Required, MaxLength(100)]
    public required string LastName { get; init; }

    [Required, EmailAddress, MaxLength(256)]
    public required string Email { get; init; }
}

/// <summary>Payload used by an administrator to reset a user's password.</summary>
public sealed record ChangeUserPasswordRequest
{
    [Required, MinLength(8), MaxLength(100)]
    public required string NewPassword { get; init; }
}

/// <summary>Safe user representation returned by the API.</summary>
public sealed record UserResponse(
    string Id,
    string? UserName,
    string? Email,
    string FirstName,
    string LastName,
    DateTimeOffset CreatedOn);