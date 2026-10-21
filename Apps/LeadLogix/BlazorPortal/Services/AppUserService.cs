using BlazorPortal.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Services;

public class AppUserService
{
    private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
    private readonly AuthenticationStateProvider authProvider;
    private readonly UserManager<ApplicationUser> userManager;


    public AppUserService(
        IDbContextFactory<ApplicationDbContext> dbFactory,
        AuthenticationStateProvider authProvider,
        UserManager<ApplicationUser> userManager)
    {
        this.dbFactory = dbFactory;
        this.authProvider = authProvider;
        this.userManager = userManager;
    }







    private async Task<string> GetClientTimeZoneInfoId()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user =authState.User;
        var appUser = await userManager.GetUserAsync(user);

        return appUser!.TimeZoneInfoId;
    }
}
