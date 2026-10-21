#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwInvoice
{
    public Guid Id { get; set; }

    public Guid AgentId { get; set; }

    public int InvoiceNo { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; }

    [Required]
    [StringLength(100)]
    [Unicode(false)]
    public string ProjectDescription { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string AgentEmail { get; set; }

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

	public decimal? DiscountAmount { get; set; }

	[Column(TypeName = "money")]
    public decimal TotalCost { get; set; }

	[Column(TypeName = "money")]
	public decimal? ReceiveAmount { get; set; }

    [NotMapped]
	public decimal? DiffAmount { get { return ReceiveAmount is not null ? TotalCost - ReceiveAmount : null; } }

	[Column(TypeName = "datetime")]
    public DateTime? PaidOn { get; set; }

    public bool IsDefault { get; set; }
}