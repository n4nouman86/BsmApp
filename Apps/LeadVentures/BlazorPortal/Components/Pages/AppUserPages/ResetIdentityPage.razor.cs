using BlazorPortal.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Components.Pages.AppUserPages;

public partial class ResetIdentityPage
{
    public IEnumerable<ApplicationUser>? ApplicationUsers { get; set; }
    public double ProgressValue { get { return field; } set; } = 0;



    protected override async Task OnInitializedAsync()
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var currentUser = await userManager.GetUserAsync(authState.User) 
            ?? throw new Exception("user not found");

        ApplicationUsers = await userManager.Users
            .Where(x => x.Id != currentUser.Id)
            .ToListAsync();


    }




    private async Task ResetIdentityAync()
    {
        var result = await dialogService
            .Confirm("Are you sure! this will reset the identity of all users?", "Reset Identity");

        if (result.HasValue == false)
        {
            return;
        }

        if (result.Value == false)
        {
            return;
        }

        if (ApplicationUsers == null)
        {
            return ; 
        }


        foreach (var user in ApplicationUsers)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var password = Guid.CreateVersion7().ToString();
            await userManager.ResetPasswordAsync(user, token, password);

            ProgressValue = ProgressValue + 1;

            StateHasChanged();
        }

        await dialogService.Alert("User's identity has been reset.", "Reset Identity");
    }
}