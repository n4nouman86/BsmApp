using BlazorPortal.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Components.Account.Pages.Manage
{
    public partial class ChangePassword
    {
        [SupplyParameterFromForm]
        private InputModel Input { get; set; } = default!;

        [CascadingParameter]
        private HttpContext HttpContext { get; set; } = default!;

        private string? message;
        private ApplicationUser? user;




        protected override async Task OnInitializedAsync()
        {
            Input ??= new();

            user = await UserManager.GetUserAsync(HttpContext.User);
            
            if (user is null)
            {
                RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
                return;
            }
        }




        private async Task OnValidSubmitAsync()
        {
            if (user is null)
            {
                RedirectManager.RedirectToInvalidUser(UserManager, HttpContext);
                return;
            }

            var changePasswordResult = await UserManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            
            if (!changePasswordResult.Succeeded)
            {
                message = $"Error: {string.Join(",", changePasswordResult.Errors.Select(error => error.Description))}";
                return;
            }

            await UserManager.Users
            .Where(x => x.Id == user.Id)
            .ExecuteUpdateAsync(setter =>
                setter.SetProperty(user => user.LastPasswordChangedDate, DateTime.Now));

            await SignInManager.RefreshSignInAsync(user);

            RedirectManager.RedirectToCurrentPageWithStatus("Your password has been changed", HttpContext);
        }




        private sealed class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; } = "";

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}