using BlazorPortal.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Components.Pages.AppUserPages;

public partial class AppUserCreatePage
{
	private InputModel Input { get; set; } = new();
	private IEnumerable<TimeZoneModel>? TimeZones { get; set; }
    private string? ErrorMessage { get; set; }



    protected override async Task OnInitializedAsync()
    {
        await using var db = await DbFactory.CreateDbContextAsync();

        TimeZones = await db.vwSqlTimeZoneInfoes
            .Select(x => new TimeZoneModel
            {
                Id = x.Name,
                Text = $"({x.CurrentUtcOffset}) (DST:{x.IsCurrentlyDst}) {x.Name}"
            })
            .ToListAsync();
    }




    public async Task RegisterUser()
    {
        ErrorMessage = null;

        var user = new ApplicationUser() 
        { 
            FullName = Input.FullName, 
            Email = Input.Email, 
            UserName = Input.Email, 
            EmailConfirmed = true,
            TimeZoneInfoId = Input.TimeZoneInfoId,
            CreateOn = DateTime.Now
        };
        
        var result = await userManager.CreateAsync(user, Input.Password);

        if (!result.Succeeded)
        {
            ErrorMessage = string.Join(", ", result.Errors.Select(error => error.Description));
            return;
        }

        result = await userManager.AddToRoleAsync(user, Input.RoleName);

        if (!result.Succeeded)
        {
			ErrorMessage = string.Join(", ", result.Errors.Select(error => error.Description));
			return;
        }

        Input = new();

        notify.Notify(Radzen.NotificationSeverity.Success, "App User", "Created successfully!");
    }




    private async Task GetSqlDateTime()
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        string sql = $"SELECT CAST(SYSDATETIMEOFFSET() AT TIME ZONE '{Input.TimeZoneInfoId}' AS DATETIME)";
        var res = await db.Database.SqlQueryRaw<DateTime>(sql).ToListAsync();
        Input.SqlDateTime = res.FirstOrDefault();
    }




    private sealed class InputModel
    {
        [Required]
        [MaxLength(100)]
		[Display(Name = "Name")]
		public string FullName { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        [MaxLength(100)]
		[Display(Name = "Time Zone")]
		public string TimeZoneInfoId { get; set; } = "";

        [Required]
        public string RoleName { get; set; } = "Agent";

        public DateTime? SqlDateTime { get; set; }
    }




    private sealed class TimeZoneModel
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}