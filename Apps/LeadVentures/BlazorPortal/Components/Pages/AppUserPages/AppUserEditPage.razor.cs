using BlazorPortal.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Components.Pages.AppUserPages;

public partial class AppUserEditPage
{
    [Parameter]
    public string Id { get; set; } = "";

	private InputUserModel InputUser { get; set; } = new();
	private ApplicationUser? AppUser { get; set; }
	private List<TimeZoneModel>? TimeZones { get; set; }

	private List<RoleModel> Roles { get; set; } = new();

	private InputPasswordModel InputPassword { get; set; } = new();

    private string? ErrorMessage { get; set; }





    protected override async Task OnInitializedAsync()
    {
        AppUser = await userManager.FindByIdAsync(Id) 
            ?? throw new Exception("user not found");

		await using var context = await DbFactory.CreateDbContextAsync();

		TimeZones = await context.vwSqlTimeZoneInfoes
			.Select(x => new TimeZoneModel
			{
				Id = x.Name,
				Text = $"({x.CurrentUtcOffset}) (DST:{x.IsCurrentlyDst}) {x.Name}"
			})
			.ToListAsync();

		InputUser.FullName = AppUser.FullName;
		InputUser.TimeZoneInfoId = AppUser.TimeZoneInfoId;
		InputUser.CheckCheckForWhitelistIps = AppUser.CheckForWhitelistIps;

		await GetSqlDateTime();

		var userHasRoles = await userManager.GetRolesAsync(AppUser);

		Roles = await roleManager.Roles
            .Select(x => new RoleModel{ RoleName = x.Name!, IsChecked = userHasRoles.Contains(x.Name!) })
            .ToListAsync();
    }




	private async Task GetSqlDateTime()
	{
		await using var db = await DbFactory.CreateDbContextAsync();
		string sql = $"SELECT CAST(SYSDATETIMEOFFSET() AT TIME ZONE '{InputUser.TimeZoneInfoId}' AS DATETIME)";
		var res = await db.Database.SqlQueryRaw<DateTime>(sql).ToListAsync();
		InputUser.SqlDateTime = res.FirstOrDefault();
	}




	private async Task UpdateAppUser()
	{
        ErrorMessage = null;

		if (AppUser is null) return;

		AppUser.FullName = InputUser.FullName;
		AppUser.TimeZoneInfoId = InputUser.TimeZoneInfoId;
		AppUser.CheckForWhitelistIps = InputUser.CheckCheckForWhitelistIps;

		var result = await userManager.UpdateAsync(AppUser);

		if (!result.Succeeded)
		{
			ErrorMessage = string.Join(", ", result.Errors.Select(error => error.Description));
			return;
		}

		notify.Notify(Radzen.NotificationSeverity.Success, "App User", "Updated successfully!");
	}




	private async Task UpdateRoles()
    {
		ErrorMessage = null;

		if (AppUser is null) return;

		if (!Roles.Where(x => x.IsChecked == true).Any())
		{
			ErrorMessage = "Select atleast one role";
			return;
		}

        var userHasRoles = await userManager.GetRolesAsync(AppUser);
        var result = await userManager.RemoveFromRolesAsync(AppUser, userHasRoles);

		if (!result.Succeeded)
		{
			ErrorMessage = string.Join(", ", result.Errors.Select(error => error.Description));
			return;
		}

		var newRoles = Roles.Where(x => x.IsChecked == true).Select(x => x.RoleName).ToList();
		result = await userManager.AddToRolesAsync(AppUser, newRoles);

		if (!result.Succeeded)
		{
			ErrorMessage = string.Join(", ", result.Errors.Select(error => error.Description));
			return;
		}

        await userManager.UpdateSecurityStampAsync(AppUser);

        notify.Notify(Radzen.NotificationSeverity.Success, "App User", "Updated successfully!");
    }




    private async Task UpdatePassword()
    {
		ErrorMessage = null;

		if (AppUser is null) return;

        var token = await userManager.GeneratePasswordResetTokenAsync(AppUser);
        var result = await userManager.ResetPasswordAsync(AppUser, token, InputPassword.Password);

		if (!result.Succeeded)
		{
			ErrorMessage = string.Join(", ", result.Errors.Select(error => error.Description));
			return;
		}

		await userManager.UpdateSecurityStampAsync(AppUser);

        await userManager.Users
			.Where(x => x.Id == AppUser.Id)
			.ExecuteUpdateAsync(setter =>
				setter.SetProperty(user => user.LastPasswordChangedDate, DateTime.Now.AddYears(-1)));

		InputPassword = new();

        notify.Notify(Radzen.NotificationSeverity.Success, "App User", "Updated successfully!");
    }




    private async Task LockedAccount()
    {
        if (AppUser is null) return;

        await userManager.SetLockoutEndDateAsync(AppUser, DateTimeOffset.MaxValue);

        await userManager.UpdateSecurityStampAsync(AppUser);

        notify.Notify(Radzen.NotificationSeverity.Success, "App User", "Locked successfully!");
    }




    private async Task UnlockedAccount()
    {
        if (AppUser is null) return;

        await userManager.SetLockoutEndDateAsync(AppUser, null);

        notify.Notify(Radzen.NotificationSeverity.Success, "App User", "Unlocked successfully!");
    }




	private sealed class InputUserModel
	{
		[Required]
		[StringLength(100)]
		[Display(Name = "Name")]
		public string FullName { get; set; } = string.Empty;

		[Required]
		[StringLength(100)]
		[Display(Name = "Time Zone")]
		public string TimeZoneInfoId { get; set; } = string.Empty;

		public DateTime? SqlDateTime { get; set; }

		public bool CheckCheckForWhitelistIps { get; set; }
	}



	private sealed class RoleModel
	{
		public string RoleName { get; set; } = string.Empty;
		public bool IsChecked { get; set; }
	}



	private sealed class InputPasswordModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }  




    private sealed class TimeZoneModel
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}