using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Data;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string TimeZoneInfoId { get; set; } = string.Empty;

    public DateTimeOffset? LastOnlineOn { get; set; }

    public DateTimeOffset CreateOn { get; set; }

    public DateTime? LastPasswordChangedDate { get; set; }

	public bool CheckForWhitelistIps { get; set; }
}
