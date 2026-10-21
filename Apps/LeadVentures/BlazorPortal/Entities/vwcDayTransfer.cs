#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BlazorPortal.Entities;

[Keyless]
public partial class vwcDayTransfer
{
    public Guid AgentId { get; set; }

    [Column(TypeName = "date")]
    public DateTime? TransferDate { get; set; }

    public int? Total { get; set; }
}