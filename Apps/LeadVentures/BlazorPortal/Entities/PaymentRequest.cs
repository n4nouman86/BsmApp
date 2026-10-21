using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlazorPortal.Entities;

public class PaymentRequest
{
	public Guid Id { get; set; }

	[Required]
	[Range(1, int.MaxValue, ErrorMessage = "Select request from.")]
	public int FromId { get; set; }

	[Required]
	public DateTime DueDate { get; set; }

	[Required]
	[MaxLength(256)]
	[Unicode(false)]
	public string Description { get; set; } = string.Empty;

	[Required]
	[MaxLength(50)]
	[Unicode(false)]
	public string ClientName { get; set; } = string.Empty;

	[Required]
	[MaxLength(50)]
	[Unicode(false)]
	public string ClientEmail { get; set; } = string.Empty;

	[Required]
	public decimal Amount { get; set; }

	public DateTime? PaidOn { get; set; }

	[MaxLength(50)]
	[Unicode(false)]
	public string? PaymentStatus { get; set; }

	public DateTime CreatedOn { get; set; }
}
