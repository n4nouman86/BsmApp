#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwUpcomingInvoice
{
    public Guid? Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string FullName { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string States { get; set; }

    public int? Billable { get; set; }

    [Column(TypeName = "money")]
    public decimal? CostPerLead { get; set; }

    [Column(TypeName = "money")]
    public decimal? TotalAmount { get; set; }
}