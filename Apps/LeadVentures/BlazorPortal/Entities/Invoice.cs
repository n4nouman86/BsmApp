using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

public partial class Invoice
{
    [Key]
    public Guid Id { get; set; }

    public int InvoiceNo { get; set; }

    public Guid AgentId { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string ProjectDescription { get; set; } = string.Empty;

    [Column(TypeName = "datetime")]
    public DateTime CreateOn { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DueDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ServiceFrom { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ServiceTo { get; set; }

    public int BillableLeads { get; set; }

    [Column(TypeName = "money")]
    public decimal CostPerLead { get; set; }

	[Column(TypeName = "money")]
	public decimal? DiscountAmount { get; set; }

	[Column(TypeName = "money")]
    public decimal TotalCost { get; set; }

	[Column(TypeName = "money")]
	public decimal? ReceiveAmount { get; set; }

	public int? InvoiceFromId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaidOn { get; set; }

	public bool IsDefault { get; set; }

	[StringLength(256)]
	[Unicode(false)]
	public string? Memo { get; set; }
}