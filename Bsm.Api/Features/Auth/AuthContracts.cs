using System.ComponentModel.DataAnnotations;

namespace Bsm.Api.Features.Auth;

/// <summary>Credentials used to sign in to the application.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}

/// <summary>Authentication result containing a bearer token and user summary.</summary>
public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    AuthUserResponse User);

/// <summary>Authenticated user details safe for the client.</summary>
public sealed record AuthUserResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles);