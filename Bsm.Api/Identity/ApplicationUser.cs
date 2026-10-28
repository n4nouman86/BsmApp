using Microsoft.AspNetCore.Identity;

namespace Bsm.Api.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.UtcNow;
}