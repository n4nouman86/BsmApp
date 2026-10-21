#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwCalledLead
{
	public int Id { get; set; }

	public DateTimeOffset CalledOn { get; set; }

	[StringLength(256)]
	[Unicode(false)]
	public string FullName { get; set; }

	[StringLength(256)]
	[Unicode(false)]
	public string PhoneNumber { get; set; }

	[StringLength(256)]
	[Unicode(false)]
	public string HomeOwner { get; set; }

	[StringLength(50)]
	[Unicode(false)]
	public string State { get; set; }

	[StringLength(256)]
	[Unicode(false)]
	public string Status { get; set; }
}