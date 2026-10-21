using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Entities;

public class PaymentRequestFrom
{
	public int Id { get; set; }

	[Required]
	[StringLength(50)]
	public string FromName { get; set; } = string.Empty;

	[Required]
	[StringLength(50)]
	public string ServerHost { get; set; } = string.Empty;

	[Required]
	public int ServerPort { get; set; }

	[Required]
	[StringLength(50)]
	public string UserName { get; set; } = string.Empty;

	[Required]
	[StringLength(50)]
	public string Password { get; set; } = string.Empty;
}
