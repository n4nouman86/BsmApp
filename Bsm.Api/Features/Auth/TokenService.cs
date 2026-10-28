using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bsm.Api.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Bsm.Api.Features.Auth;

public interface ITokenService
{
    Task<AuthResponse> CreateAsync(ApplicationUser user);
}

public sealed class TokenService(UserManager<ApplicationUser> userManager, IConfiguration configuration) : ITokenService
{
    public async Task<AuthResponse> CreateAsync(ApplicationUser user)
    {
        var jwt = configuration.GetSection("Jwt");
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(jwt.GetValue<int>("ExpiryMinutes", 720));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new("first_name", user.FirstName),
            new("last_name", user.LastName),
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            SecurityAlgorithms.HmacSha256);
        // notBefore lets the middleware work out how much of the token's lifetime has passed.
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new AuthUserResponse(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToArray()));
    }
}
