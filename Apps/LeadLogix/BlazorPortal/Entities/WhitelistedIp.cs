using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Entities;

public class WhitelistedIp
{
	public int Id { get; set; }

	[Required]
	[StringLength(45)]
	public string IpAddress { get; set; } = string.Empty;

	[StringLength(250)]
	public string? Description { get; set; }

	public DateTime CreatedOn { get; set; } = DateTime.Now;
}
