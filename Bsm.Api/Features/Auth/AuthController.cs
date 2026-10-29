using Bsm.Api.Controllers;
using Bsm.Api.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bsm.Api.Features.Auth;

public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IWebHostEnvironment environment) : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsBlocked || (!environment.IsDevelopment()
            && string.Equals(user.Email, "demo@bsmjoin.test", StringComparison.OrdinalIgnoreCase)))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(await tokenService.CreateAsync(user));
    }
}
