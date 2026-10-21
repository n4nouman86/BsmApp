namespace BlazorPortal.Entities;

public class vwPaymentRequest
{
	public Guid Id { get; set; }
	public int FromId { get; set; }
	public string FromName { get; set; }= string.Empty;
	public DateTime DueDate { get; set; }
	public string ClientName { get; set; }=string.Empty;
	public string ClientEmail { get; set; } = string.Empty;
	public decimal Amount { get; set; }
	public string Description { get; set; } = string.Empty;
	public DateTime? PaidOn { get; set; }
	public string? PaymentStatus { get; set; }
	public DateTime CreatedOn { get; set; }
}
